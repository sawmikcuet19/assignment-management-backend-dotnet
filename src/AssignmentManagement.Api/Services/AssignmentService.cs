using AssignmentManagement.Api.Auth;
using AssignmentManagement.Api.Data;
using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Api.Services;

public class AssignmentService
{
    private readonly AppDbContext db;
    private readonly CurrentUser currentUser;

    public AssignmentService(AppDbContext db, CurrentUser currentUser)
    {
        this.db = db;
        this.currentUser = currentUser;
    }

    public async Task<List<AssignmentListItemDto>> GetAssignments(AssignmentStatus? status)
    {
        var query = db.Assignments.AsNoTracking().AsQueryable();

        if (currentUser.IsTeacher)
        {
            query = query.Where(x =>
                x.ClassSubject!.TeacherAssignments.Any(t =>
                    t.TeacherUserId == currentUser.Id &&
                    t.IsActive));
        }
        else if (currentUser.IsStudent)
        {
            query = query.Where(x =>
                x.Status == AssignmentStatus.Published &&
                x.ClassSubject!.Class!.StudentEnrollments.Any(sc =>
                    sc.StudentUserId == currentUser.Id &&
                    sc.IsActive));
        }
        else if (!currentUser.IsAdmin)
        {
            throw new UnauthorizedAccessException("Invalid role.");
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.DeadlineUtc)
            .Select(x => new AssignmentListItemDto(
                x.Id,
                x.ClassSubjectId,
                x.Title,
                x.Status,
                x.MaxMarks,
                x.DeadlineUtc,
                x.StartsAtUtc,
                x.ClassSubject!.Class!.Name,
                x.ClassSubject.Subject!.Name
            ))
            .ToListAsync();
    }

    public async Task<List<ClassSubjectDetailDto>> GetTeacherClassSubjects()
    {
        if (!currentUser.IsTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can list their class subjects.");
        }

        return await db.ClassSubjects
            .AsNoTracking()
            .Where(x => x.IsActive &&
                x.TeacherAssignments.Any(t =>
                    t.TeacherUserId == currentUser.Id &&
                    t.IsActive))
            .OrderBy(x => x.Class!.Name)
            .ThenBy(x => x.Subject!.Name)
            .Select(x => new ClassSubjectDetailDto(
                x.Id,
                x.ClassId,
                x.Class!.Name,
                x.SubjectId,
                x.Subject!.Name,
                x.AcademicYear,
                x.IsActive
            ))
            .ToListAsync();
    }

    public async Task<AssignmentDetailsDto> GetAssignmentById(int id)
    {
        var assignment = await db.Assignments
            .AsNoTracking()
            .Include(x => x.ClassSubject)!
            .ThenInclude(x => x!.Class)
            .Include(x => x.ClassSubject)!
            .ThenInclude(x => x!.Subject)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (currentUser.IsTeacher)
        {
            var assigned = await db.TeacherClassSubjects.AnyAsync(x =>
                x.ClassSubjectId == assignment.ClassSubjectId &&
                x.TeacherUserId == currentUser.Id &&
                x.IsActive);

            if (!assigned)
            {
                throw new KeyNotFoundException("Assignment not found.");
            }
        }

        if (currentUser.IsStudent)
        {
            if (assignment.Status != AssignmentStatus.Published)
            {
                throw new KeyNotFoundException("Assignment not found.");
            }

            var enrolled = await db.StudentClasses.AnyAsync(x =>
                x.ClassId == assignment.ClassSubject!.ClassId &&
                x.StudentUserId == currentUser.Id &&
                x.IsActive);

            if (!enrolled)
            {
                throw new KeyNotFoundException("Assignment not found.");
            }
        }

        return MapAssignment(assignment);
    }

    public async Task<AssignmentDetailsDto> CreateAssignment(CreateAssignmentRequest request)
    {
        if (!currentUser.IsTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can create assignments.");
        }

        var classSubject = await db.ClassSubjects
            .Include(x => x.TeacherAssignments)
            .FirstOrDefaultAsync(x => x.Id == request.ClassSubjectId && x.IsActive)
            ?? throw new KeyNotFoundException("Class subject not found.");

        var assigned = classSubject.TeacherAssignments.Any(x =>
            x.TeacherUserId == currentUser.Id &&
            x.IsActive);

        if (!assigned)
        {
            throw new UnauthorizedAccessException("You are not assigned to this class subject.");
        }

        if (request.MaxMarks <= 0)
        {
            throw new InvalidOperationException("Maximum marks must be greater than zero.");
        }

        var deadlineUtc = DateTime.SpecifyKind(request.DeadlineUtc, DateTimeKind.Utc);

        if (deadlineUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Deadline must be in the future.");
        }

        var startsAtUtc = NormalizeStartTime(request.StartsAtUtc, deadlineUtc);

        var assignment = new Assignment
        {
            ClassSubjectId = request.ClassSubjectId,
            CreatedByUserId = currentUser.Id,
            Title = request.Title,
            Description = request.Description,
            MaxMarks = request.MaxMarks,
            DeadlineUtc = deadlineUtc,
            StartsAtUtc = startsAtUtc,
            AllowUpdateBeforeDeadline = request.AllowUpdateBeforeDeadline,
            Status = AssignmentStatus.Draft
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        return await GetAssignmentById(assignment.Id);
    }

    public async Task<AssignmentDetailsDto> UpdateAssignment(int id, UpdateAssignmentRequest request)
    {
        var assignment = await db.Assignments
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        await EnsureTeacherCanManage(assignment.ClassSubjectId);

        if (request.MaxMarks <= 0)
        {
            throw new InvalidOperationException("Maximum marks must be greater than zero.");
        }

        var deadlineUtc = DateTime.SpecifyKind(request.DeadlineUtc, DateTimeKind.Utc);

        if (deadlineUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Deadline must be in the future.");
        }

        var startsAtUtc = NormalizeStartTime(request.StartsAtUtc, deadlineUtc);

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.MaxMarks = request.MaxMarks;
        assignment.DeadlineUtc = deadlineUtc;
        assignment.StartsAtUtc = startsAtUtc;
        assignment.AllowUpdateBeforeDeadline = request.AllowUpdateBeforeDeadline;
        assignment.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetAssignmentById(assignment.Id);
    }

    public async Task PublishAssignment(int id)
    {
        var assignment = await db.Assignments
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        await EnsureTeacherCanManage(assignment.ClassSubjectId);

        if (string.IsNullOrWhiteSpace(assignment.Title))
        {
            throw new InvalidOperationException("Assignment title is required.");
        }

        if (string.IsNullOrWhiteSpace(assignment.Description))
        {
            throw new InvalidOperationException("Assignment description is required.");
        }

        if (assignment.MaxMarks <= 0)
        {
            throw new InvalidOperationException("Maximum marks must be greater than zero.");
        }

        if (assignment.DeadlineUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Deadline must be in the future.");
        }

        assignment.Status = AssignmentStatus.Published;
        assignment.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task ArchiveAssignment(int id)
    {
        var assignment = await db.Assignments
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        await EnsureTeacherCanManage(assignment.ClassSubjectId);

        assignment.Status = AssignmentStatus.Archived;
        assignment.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task UnarchiveAssignment(int id)
    {
        var assignment = await db.Assignments
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        await EnsureTeacherCanManage(assignment.ClassSubjectId);

        if (assignment.Status != AssignmentStatus.Archived)
        {
            throw new InvalidOperationException("Only archived assignments can be unarchived.");
        }

        if (string.IsNullOrWhiteSpace(assignment.Title))
        {
            throw new InvalidOperationException("Assignment title is required.");
        }

        if (string.IsNullOrWhiteSpace(assignment.Description))
        {
            throw new InvalidOperationException("Assignment description is required.");
        }

        if (assignment.MaxMarks <= 0)
        {
            throw new InvalidOperationException("Maximum marks must be greater than zero.");
        }

        if (assignment.DeadlineUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Deadline must be in the future.");
        }

        assignment.Status = AssignmentStatus.Published;
        assignment.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task<string> DeleteAssignment(int id)
    {
        var assignment = await db.Assignments
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        await EnsureTeacherCanManage(assignment.ClassSubjectId);

        var hasSubmissions = await db.Submissions
            .AnyAsync(x => x.AssignmentId == id);

        if (!hasSubmissions)
        {
            db.Assignments.Remove(assignment);
            await db.SaveChangesAsync();
            return "Assignment deleted.";
        }

        assignment.Status = AssignmentStatus.Archived;
        assignment.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return "Assignment has submissions, so it was archived instead of deleted.";
    }

    private async Task EnsureTeacherCanManage(int classSubjectId)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        if (!currentUser.IsTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can manage assignments.");
        }

        var assigned = await db.TeacherClassSubjects.AnyAsync(x =>
            x.ClassSubjectId == classSubjectId &&
            x.TeacherUserId == currentUser.Id &&
            x.IsActive);

        if (!assigned)
        {
            throw new UnauthorizedAccessException("You are not assigned to this class subject.");
        }
    }

    private static DateTime? NormalizeStartTime(DateTime? startsAtUtc, DateTime deadlineUtc)
    {
        if (!startsAtUtc.HasValue)
        {
            return null;
        }

        var start = DateTime.SpecifyKind(startsAtUtc.Value, DateTimeKind.Utc);

        if (start >= deadlineUtc)
        {
            throw new InvalidOperationException("Start time must be before the deadline.");
        }

        return start;
    }

    private static AssignmentDetailsDto MapAssignment(Assignment assignment)
    {
        return new AssignmentDetailsDto(
            assignment.Id,
            assignment.ClassSubjectId,
            assignment.Title,
            assignment.Description,
            assignment.MaxMarks,
            assignment.DeadlineUtc,
            assignment.StartsAtUtc,
            assignment.Status,
            assignment.AllowUpdateBeforeDeadline,
            assignment.CreatedAtUtc,
            assignment.UpdatedAtUtc,
            assignment.ClassSubject?.Class?.Name ?? string.Empty,
            assignment.ClassSubject?.Subject?.Name ?? string.Empty
        );
    }
}