using AssignmentManagement.Api.Auth;
using AssignmentManagement.Api.Data;
using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Api.Services;

public class SubmissionService
{
    private readonly AppDbContext db;
    private readonly CurrentUser currentUser;

    public SubmissionService(AppDbContext db, CurrentUser currentUser)
    {
        this.db = db;
        this.currentUser = currentUser;
    }

    public async Task<SubmissionDto> Submit(int assignmentId, CreateSubmissionRequest request)
    {
        if (!currentUser.IsStudent)
        {
            throw new UnauthorizedAccessException("Only students can submit assignments.");
        }

        var assignment = await db.Assignments
            .AsNoTracking()
            .Include(x => x.ClassSubject)
            .FirstOrDefaultAsync(x => x.Id == assignmentId)
            ?? throw new KeyNotFoundException("Assignment not found.");

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
            throw new UnauthorizedAccessException("You are not enrolled in this class.");
        }

        if (assignment.DeadlineUtc < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Submission deadline has passed.");
        }

        var alreadySubmitted = await db.Submissions.AnyAsync(x =>
            x.AssignmentId == assignmentId &&
            x.StudentUserId == currentUser.Id);

        if (alreadySubmitted)
        {
            throw new InvalidOperationException("You have already submitted this assignment.");
        }

        var student = await db.Users.AsNoTracking()
            .FirstAsync(x => x.Id == currentUser.Id);

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentUserId = currentUser.Id,
            AnswerText = request.AnswerText,
            Status = SubmissionStatus.Submitted,
            SubmittedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        return new SubmissionDto(
            submission.Id,
            submission.AssignmentId,
            submission.StudentUserId,
            student.FullName,
            submission.AnswerText,
            submission.Status,
            submission.MarksObtained,
            submission.Feedback,
            submission.SubmittedAtUtc,
            submission.UpdatedAtUtc
        );
    }

    public async Task<SubmissionDto> UpdateSubmission(int submissionId, UpdateSubmissionRequest request)
    {
        var submission = await db.Submissions
            .Include(x => x.Assignment)
            .FirstOrDefaultAsync(x =>
                x.Id == submissionId &&
                x.StudentUserId == currentUser.Id)
            ?? throw new KeyNotFoundException("Submission not found.");

        var assignment = submission.Assignment!;

        if (assignment.Status != AssignmentStatus.Published)
        {
            throw new InvalidOperationException("Assignment is not published.");
        }

        if (submission.Status == SubmissionStatus.Graded ||
            submission.Status == SubmissionStatus.Returned)
        {
            throw new InvalidOperationException("Submission can no longer be updated.");
        }

        if (!assignment.AllowUpdateBeforeDeadline)
        {
            throw new InvalidOperationException("Submission updates are not allowed.");
        }

        if (DateTime.UtcNow > assignment.DeadlineUtc)
        {
            throw new InvalidOperationException("Submission deadline has passed.");
        }

        submission.AnswerText = request.AnswerText;
        submission.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetSubmissionDtoById(submission.Id);
    }

    public async Task<SubmissionDto?> GetMySubmission(int assignmentId)
    {
        if (!currentUser.IsStudent)
        {
            throw new UnauthorizedAccessException("Only students can use this endpoint.");
        }

        var submission = await db.Submissions
            .AsNoTracking()
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x =>
                x.AssignmentId == assignmentId &&
                x.StudentUserId == currentUser.Id);

        if (submission is null)
        {
            return null;
        }

        return MapSubmission(submission);
    }

    public async Task<List<SubmissionDto>> GetSubmissionsForAssignment(int assignmentId)
    {
        var assignment = await GetAssignmentForTeacherOrAdmin(assignmentId);

        return await db.Submissions
            .AsNoTracking()
            .Include(x => x.Student)
            .Where(x => x.AssignmentId == assignment.Id)
            .Select(x => new SubmissionDto(
                x.Id,
                x.AssignmentId,
                x.StudentUserId,
                x.Student!.FullName,
                x.AnswerText,
                x.Status,
                x.MarksObtained,
                x.Feedback,
                x.SubmittedAtUtc,
                x.UpdatedAtUtc
            ))
            .ToListAsync();
    }

    public async Task<SubmissionDto> GetSubmissionById(int submissionId)
    {
        var submission = await db.Submissions
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Assignment)
            .FirstOrDefaultAsync(x => x.Id == submissionId)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (currentUser.IsAdmin)
        {
            return MapSubmission(submission);
        }

        if (currentUser.IsStudent && submission.StudentUserId == currentUser.Id)
        {
            return MapSubmission(submission);
        }

        if (currentUser.IsTeacher)
        {
            var assigned = await db.TeacherClassSubjects.AnyAsync(x =>
                x.ClassSubjectId == submission.Assignment!.ClassSubjectId &&
                x.TeacherUserId == currentUser.Id &&
                x.IsActive);

            if (assigned)
            {
                return MapSubmission(submission);
            }
        }

        throw new UnauthorizedAccessException("You cannot view this submission.");
    }

    public async Task<SubmissionDto> GradeSubmission(int submissionId, GradeSubmissionRequest request)
    {
        var submission = await db.Submissions
            .Include(x => x.Assignment)
            .FirstOrDefaultAsync(x => x.Id == submissionId)
            ?? throw new KeyNotFoundException("Submission not found.");

        await EnsureTeacherCanManage(submission.Assignment!.ClassSubjectId);

        if (request.MarksObtained < 0)
        {
            throw new InvalidOperationException("Marks cannot be negative.");
        }

        if (request.MarksObtained > submission.Assignment.MaxMarks)
        {
            throw new InvalidOperationException("Marks cannot exceed maximum marks.");
        }

        if (request.Status == SubmissionStatus.Submitted)
        {
            throw new InvalidOperationException("A submission cannot be graded back to 'Submitted'.");
        }

        submission.MarksObtained = request.MarksObtained;
        submission.Feedback = request.Feedback;
        submission.Status = request.Status;
        submission.ReviewedByUserId = currentUser.Id;
        submission.ReviewedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetSubmissionDtoById(submission.Id);
    }

    private async Task<Assignment> GetAssignmentForTeacherOrAdmin(int assignmentId)
    {
        var assignment = await db.Assignments
            .AsNoTracking()
            .Include(x => x.ClassSubject)
            .FirstOrDefaultAsync(x => x.Id == assignmentId)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (currentUser.IsAdmin)
        {
            return assignment;
        }

        if (!currentUser.IsTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can view submissions.");
        }

        var assigned = await db.TeacherClassSubjects.AnyAsync(x =>
            x.ClassSubjectId == assignment.ClassSubjectId &&
            x.TeacherUserId == currentUser.Id &&
            x.IsActive);

        if (!assigned)
        {
            throw new UnauthorizedAccessException("You are not assigned to this class subject.");
        }

        return assignment;
    }

    private async Task EnsureTeacherCanManage(int classSubjectId)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        if (!currentUser.IsTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can manage submissions.");
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

    private async Task<SubmissionDto> GetSubmissionDtoById(int submissionId)
    {
        var submission = await db.Submissions
            .AsNoTracking()
            .Include(x => x.Student)
            .FirstAsync(x => x.Id == submissionId);

        return MapSubmission(submission);
    }

    private static SubmissionDto MapSubmission(Submission submission)
    {
        return new SubmissionDto(
            submission.Id,
            submission.AssignmentId,
            submission.StudentUserId,
            submission.Student?.FullName ?? string.Empty,
            submission.AnswerText,
            submission.Status,
            submission.MarksObtained,
            submission.Feedback,
            submission.SubmittedAtUtc,
            submission.UpdatedAtUtc
        );
    }
}