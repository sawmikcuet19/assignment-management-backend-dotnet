using AssignmentManagement.Api.Data;
using AssignmentManagement.Api.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Tests;

public static class TestDb
{
    public static (AppDbContext Db, SqliteConnection Connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        SeedRoles(db);

        return (db, connection);
    }

    private static void SeedRoles(AppDbContext db)
    {
        db.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Teacher" },
            new Role { Name = "Student" });
        db.SaveChanges();
    }
}

public static class TestData
{
    public static User CreateUser(
        AppDbContext db,
        string role,
        string email,
        string fullName = "Test User")
    {
        var roleEntity = db.Roles.Single(x => x.Name == role);
        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            RoleId = roleEntity.Id,
            Role = roleEntity,
            IsActive = true
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static ClassSubject CreateClassSubject(
        AppDbContext db,
        string className = "Class 9",
        string subjectName = "Mathematics")
    {
        var cls = new ClassCourse
        {
            Name = className,
            Code = "C",
            Description = className,
            IsActive = true
        };
        var subject = new Subject
        {
            Name = subjectName,
            Code = "S",
            Description = subjectName,
            IsActive = true
        };
        db.AddRange(cls, subject);
        db.SaveChanges();

        var classSubject = new ClassSubject
        {
            ClassId = cls.Id,
            SubjectId = subject.Id,
            AcademicYear = "2026",
            IsActive = true
        };
        db.ClassSubjects.Add(classSubject);
        db.SaveChanges();

        return classSubject;
    }

    public static void AssignTeacher(AppDbContext db, int classSubjectId, int teacherUserId)
    {
        db.TeacherClassSubjects.Add(new TeacherClassSubject
        {
            ClassSubjectId = classSubjectId,
            TeacherUserId = teacherUserId,
            IsActive = true
        });
        db.SaveChanges();
    }

    public static void EnrollStudent(AppDbContext db, int classId, int studentUserId)
    {
        db.StudentClasses.Add(new StudentClass
        {
            ClassId = classId,
            StudentUserId = studentUserId,
            AcademicYear = "2026",
            IsActive = true
        });
        db.SaveChanges();
    }

    public static Assignment CreateAssignment(
        AppDbContext db,
        int classSubjectId,
        int teacherUserId,
        string title = "Homework",
        AssignmentStatus status = AssignmentStatus.Published,
        int maxMarks = 100,
        DateTime? deadlineUtc = null,
        DateTime? startsAtUtc = null,
        bool allowUpdateBeforeDeadline = true)
    {
        var assignment = new Assignment
        {
            ClassSubjectId = classSubjectId,
            CreatedByUserId = teacherUserId,
            Title = title,
            Description = "Description",
            MaxMarks = maxMarks,
            DeadlineUtc = deadlineUtc ?? DateTime.UtcNow.AddDays(7),
            StartsAtUtc = startsAtUtc,
            Status = status,
            AllowUpdateBeforeDeadline = allowUpdateBeforeDeadline
        };
        db.Assignments.Add(assignment);
        db.SaveChanges();
        return assignment;
    }

    public static void Dispose(AppDbContext db, SqliteConnection connection)
    {
        db.Dispose();
        connection.Dispose();
    }
}