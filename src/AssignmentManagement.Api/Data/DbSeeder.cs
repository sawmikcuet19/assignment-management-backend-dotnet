using AssignmentManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Api.Data;

public static class DbSeeder
{
    public static async Task Seed(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Teacher" },
                new Role { Name = "Student" }
            );

            await db.SaveChangesAsync();
        }

        var adminRole = await db.Roles.FirstAsync(x => x.Name == "Admin");
        var teacherRole = await db.Roles.FirstAsync(x => x.Name == "Teacher");
        var studentRole = await db.Roles.FirstAsync(x => x.Name == "Student");

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User
                {
                    FullName = "System Admin",
                    Email = "admin@school.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    RoleId = adminRole.Id,
                    IsActive = true
                },
                new User
                {
                    FullName = "Demo Teacher",
                    Email = "teacher@school.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    RoleId = teacherRole.Id,
                    IsActive = true
                },
                new User
                {
                    FullName = "Demo Student",
                    Email = "student@school.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    RoleId = studentRole.Id,
                    IsActive = true
                }
            );

            await db.SaveChangesAsync();
        }

        if (!await db.ClassCourses.AnyAsync())
        {
            db.ClassCourses.Add(new ClassCourse
            {
                Name = "Class 9",
                Code = "C9",
                Description = "Class 9 course",
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        if (!await db.Subjects.AnyAsync())
        {
            db.Subjects.Add(new Subject
            {
                Name = "Mathematics",
                Code = "MATH",
                Description = "Mathematics subject",
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        var classCourse = await db.ClassCourses.FirstAsync(x => x.Name == "Class 9");
        var subject = await db.Subjects.FirstAsync(x => x.Name == "Mathematics");

        var classSubject = await db.ClassSubjects.FirstOrDefaultAsync(x =>
            x.ClassId == classCourse.Id &&
            x.SubjectId == subject.Id);

        if (classSubject is null)
        {
            classSubject = new ClassSubject
            {
                ClassId = classCourse.Id,
                SubjectId = subject.Id,
                AcademicYear = "2026",
                IsActive = true
            };

            db.ClassSubjects.Add(classSubject);
            await db.SaveChangesAsync();
        }

        var teacher = await db.Users.FirstAsync(x => x.Email == "teacher@school.local");
        var student = await db.Users.FirstAsync(x => x.Email == "student@school.local");

        var teacherAssigned = await db.TeacherClassSubjects.AnyAsync(x =>
            x.ClassSubjectId == classSubject.Id &&
            x.TeacherUserId == teacher.Id);

        if (!teacherAssigned)
        {
            db.TeacherClassSubjects.Add(new TeacherClassSubject
            {
                ClassSubjectId = classSubject.Id,
                TeacherUserId = teacher.Id,
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        var studentEnrolled = await db.StudentClasses.AnyAsync(x =>
            x.ClassId == classCourse.Id &&
            x.StudentUserId == student.Id);

        if (!studentEnrolled)
        {
            db.StudentClasses.Add(new StudentClass
            {
                ClassId = classCourse.Id,
                StudentUserId = student.Id,
                AcademicYear = "2026",
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        if (!await db.Assignments.AnyAsync())
        {
            db.Assignments.Add(new Assignment
            {
                ClassSubjectId = classSubject.Id,
                CreatedByUserId = teacher.Id,
                Title = "Chapter 1 Homework",
                Description = "Complete all exercises from chapter 1.",
                MaxMarks = 100,
                DeadlineUtc = DateTime.UtcNow.AddDays(7),
                StartsAtUtc = DateTime.UtcNow.AddHours(-1),
                Status = AssignmentStatus.Published,
                AllowUpdateBeforeDeadline = true
            });

            await db.SaveChangesAsync();
        }
    }
}