using AssignmentManagement.Api.Auth;
using AssignmentManagement.Api.Data;
using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Api.Services;

public class AdminService
{
    private readonly AppDbContext db;
    private readonly CurrentUser currentUser;

    public AdminService(AppDbContext db, CurrentUser currentUser)
    {
        this.db = db;
        this.currentUser = currentUser;
    }

    #region Users

    public async Task<List<UserResponse>> GetUsers()
    {
        return await db.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .Select(x => new UserResponse(
                x.Id,
                x.FullName,
                x.Email,
                x.Role!.Name,
                x.IsActive
            ))
            .ToListAsync();
    }

    public async Task<UserResponse> CreateUser(CreateUserRequest request)
    {
        var role = await db.Roles
            .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Role.ToLower())
            ?? throw new KeyNotFoundException($"Role '{request.Role}' not found.");

        var emailExists = await db.Users
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id,
            Role = role,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return MapUser(user);
    }

    public async Task<UserResponse> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("User not found.");

        var role = await db.Roles
            .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Role.ToLower())
            ?? throw new KeyNotFoundException($"Role '{request.Role}' not found.");

        var emailExists = await db.Users
            .AnyAsync(x => x.Email == request.Email && x.Id != id);

        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.RoleId = role.Id;
        user.Role = role;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await db.SaveChangesAsync();

        return MapUser(user);
    }

    public async Task DeactivateUser(int id)
    {
        var user = await db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Id == currentUser.Id)
        {
            throw new InvalidOperationException("You cannot deactivate your own account.");
        }

        user.IsActive = false;
        await db.SaveChangesAsync();
    }

    private static UserResponse MapUser(User user)
    {
        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role?.Name ?? string.Empty,
            user.IsActive
        );
    }

    #endregion

    #region Classes

    public async Task<List<ClassResponse>> GetClasses()
    {
        return await db.ClassCourses
            .AsNoTracking()
            .Select(x => new ClassResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive
            ))
            .ToListAsync();
    }

    public async Task<ClassResponse> CreateClass(CreateClassRequest request)
    {
        var classCourse = new ClassCourse
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = true
        };

        db.ClassCourses.Add(classCourse);
        await db.SaveChangesAsync();

        return new ClassResponse(
            classCourse.Id,
            classCourse.Name,
            classCourse.Code,
            classCourse.Description,
            classCourse.IsActive
        );
    }

    public async Task<ClassResponse> UpdateClass(int id, UpdateClassRequest request)
    {
        var classCourse = await db.ClassCourses.FindAsync(id)
            ?? throw new KeyNotFoundException("Class not found.");

        classCourse.Name = request.Name;
        classCourse.Code = request.Code;
        classCourse.Description = request.Description;
        classCourse.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return new ClassResponse(
            classCourse.Id,
            classCourse.Name,
            classCourse.Code,
            classCourse.Description,
            classCourse.IsActive
        );
    }

    public async Task DeactivateClass(int id)
    {
        var classCourse = await db.ClassCourses.FindAsync(id)
            ?? throw new KeyNotFoundException("Class not found.");

        classCourse.IsActive = false;
        await db.SaveChangesAsync();
    }

    #endregion

    #region Subjects

    public async Task<List<SubjectResponse>> GetSubjects()
    {
        return await db.Subjects
            .AsNoTracking()
            .Select(x => new SubjectResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive
            ))
            .ToListAsync();
    }

    public async Task<SubjectResponse> CreateSubject(CreateSubjectRequest request)
    {
        var subject = new Subject
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = true
        };

        db.Subjects.Add(subject);
        await db.SaveChangesAsync();

        return new SubjectResponse(
            subject.Id,
            subject.Name,
            subject.Code,
            subject.Description,
            subject.IsActive
        );
    }

    public async Task<SubjectResponse> UpdateSubject(int id, UpdateSubjectRequest request)
    {
        var subject = await db.Subjects.FindAsync(id)
            ?? throw new KeyNotFoundException("Subject not found.");

        subject.Name = request.Name;
        subject.Code = request.Code;
        subject.Description = request.Description;
        subject.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return new SubjectResponse(
            subject.Id,
            subject.Name,
            subject.Code,
            subject.Description,
            subject.IsActive
        );
    }

    public async Task DeactivateSubject(int id)
    {
        var subject = await db.Subjects.FindAsync(id)
            ?? throw new KeyNotFoundException("Subject not found.");

        subject.IsActive = false;
        await db.SaveChangesAsync();
    }

    #endregion

    #region Class Subjects

    public async Task<List<ClassSubjectDetailDto>> GetClassSubjects()
    {
        return await db.ClassSubjects
            .AsNoTracking()
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

    public async Task<ClassSubjectDetailDto> CreateClassSubject(CreateClassSubjectRequest request)
    {
        var classExists = await db.ClassCourses.AnyAsync(x => x.Id == request.ClassId);
        var subjectExists = await db.Subjects.AnyAsync(x => x.Id == request.SubjectId);

        if (!classExists)
        {
            throw new KeyNotFoundException("Class not found.");
        }

        if (!subjectExists)
        {
            throw new KeyNotFoundException("Subject not found.");
        }

        var exists = await db.ClassSubjects.AnyAsync(x =>
            x.ClassId == request.ClassId &&
            x.SubjectId == request.SubjectId &&
            x.AcademicYear == request.AcademicYear);

        if (exists)
        {
            throw new InvalidOperationException("This class subject already exists.");
        }

        var classSubject = new ClassSubject
        {
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            AcademicYear = request.AcademicYear,
            IsActive = true
        };

        db.ClassSubjects.Add(classSubject);
        await db.SaveChangesAsync();

        return await GetClassSubjectDto(classSubject.Id);
    }

    public async Task<ClassSubjectDetailDto> UpdateClassSubject(int id, UpdateClassSubjectRequest request)
    {
        var classSubject = await db.ClassSubjects.FindAsync(id)
            ?? throw new KeyNotFoundException("Class subject not found.");

        classSubject.AcademicYear = request.AcademicYear;
        classSubject.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return await GetClassSubjectDto(classSubject.Id);
    }

    public async Task DeactivateClassSubject(int id)
    {
        var classSubject = await db.ClassSubjects.FindAsync(id)
            ?? throw new KeyNotFoundException("Class subject not found.");

        classSubject.IsActive = false;
        await db.SaveChangesAsync();
    }

    private async Task<ClassSubjectDetailDto> GetClassSubjectDto(int id)
    {
        return await db.ClassSubjects
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ClassSubjectDetailDto(
                x.Id,
                x.ClassId,
                x.Class!.Name,
                x.SubjectId,
                x.Subject!.Name,
                x.AcademicYear,
                x.IsActive
            ))
            .FirstAsync();
    }

    #endregion

    #region Teacher Assignment

    public async Task AssignTeacher(int classSubjectId, int teacherUserId)
    {
        var classSubject = await db.ClassSubjects.FindAsync(classSubjectId)
            ?? throw new KeyNotFoundException("Class subject not found.");

        var teacher = await db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == teacherUserId)
            ?? throw new KeyNotFoundException("Teacher not found.");

        if (!string.Equals(teacher.Role?.Name, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Selected user is not a teacher.");
        }

        var existing = await db.TeacherClassSubjects
            .FirstOrDefaultAsync(x =>
                x.ClassSubjectId == classSubjectId &&
                x.TeacherUserId == teacherUserId);

        if (existing is null)
        {
            db.TeacherClassSubjects.Add(new TeacherClassSubject
            {
                ClassSubjectId = classSubjectId,
                TeacherUserId = teacherUserId,
                IsActive = true
            });
        }
        else
        {
            existing.IsActive = true;
        }

        await db.SaveChangesAsync();
    }

    public async Task RemoveTeacher(int classSubjectId, int teacherUserId)
    {
        var existing = await db.TeacherClassSubjects
            .FirstOrDefaultAsync(x =>
                x.ClassSubjectId == classSubjectId &&
                x.TeacherUserId == teacherUserId)
            ?? throw new KeyNotFoundException("Teacher assignment not found.");

        db.TeacherClassSubjects.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<List<UserResponse>> GetClassSubjectTeachers(int classSubjectId)
    {
        var classSubject = await db.ClassSubjects
            .AsNoTracking()
            .Include(x => x.TeacherAssignments)
            .ThenInclude(x => x.Teacher)!
            .ThenInclude(x => x!.Role)
            .FirstOrDefaultAsync(x => x.Id == classSubjectId)
            ?? throw new KeyNotFoundException("Class subject not found.");

        return classSubject.TeacherAssignments
            .Where(x => x.IsActive && x.Teacher is not null)
            .Select(x => MapUser(x.Teacher!))
            .ToList();
    }

    #endregion

    #region Student Enrollment

    public async Task EnrollStudent(int classId, int studentUserId)
    {
        var classExists = await db.ClassCourses.AnyAsync(x => x.Id == classId);

        if (!classExists)
        {
            throw new KeyNotFoundException("Class not found.");
        }

        var student = await db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == studentUserId)
            ?? throw new KeyNotFoundException("Student not found.");

        if (!string.Equals(student.Role?.Name, "Student", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Selected user is not a student.");
        }

        var exists = await db.StudentClasses
            .AnyAsync(x => x.ClassId == classId && x.StudentUserId == studentUserId);

        if (exists)
        {
            throw new InvalidOperationException("Student is already enrolled in this class.");
        }

        db.StudentClasses.Add(new StudentClass
        {
            ClassId = classId,
            StudentUserId = studentUserId,
            IsActive = true
        });

        await db.SaveChangesAsync();
    }

    public async Task RemoveStudent(int classId, int studentUserId)
    {
        var enrollment = await db.StudentClasses
            .FirstOrDefaultAsync(x =>
                x.ClassId == classId &&
                x.StudentUserId == studentUserId)
            ?? throw new KeyNotFoundException("Student enrollment not found.");

        db.StudentClasses.Remove(enrollment);
        await db.SaveChangesAsync();
    }

    public async Task<List<UserResponse>> GetClassStudents(int classId)
    {
        return await db.StudentClasses
            .AsNoTracking()
            .Where(x => x.ClassId == classId && x.IsActive)
            .Select(x => new UserResponse(
                x.Student!.Id,
                x.Student.FullName,
                x.Student.Email,
                x.Student.Role!.Name,
                x.Student.IsActive
            ))
            .ToListAsync();
    }

    #endregion
}