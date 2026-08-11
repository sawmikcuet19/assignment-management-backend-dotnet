using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services;
using Microsoft.Data.Sqlite;

namespace AssignmentManagement.Tests;

public class AssignmentServiceTests
{
    [Fact]
    public async Task GetAssignments_ForStudent_OnlyReturnsPublishedAssignmentsForEnrolledClass()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db, "Class 9", "Mathematics");
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);

            var draft = TestData.CreateAssignment(db, classSubject.Id, teacher.Id,
                title: "Draft assignment", status: AssignmentStatus.Draft);
            var published = TestData.CreateAssignment(db, classSubject.Id, teacher.Id,
                title: "Published assignment", status: AssignmentStatus.Published);

            var service = new AssignmentService(db, TestServices.As(student));

            var result = await service.GetAssignments(null);

            Assert.Contains(result, x => x.Id == published.Id);
            Assert.DoesNotContain(result, x => x.Id == draft.Id);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GetAssignments_ForStudent_ExcludesUnenrolledClass()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db, "Class 10", "Science");
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);

            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var service = new AssignmentService(db, TestServices.As(student));

            var result = await service.GetAssignments(null);

            Assert.DoesNotContain(result, x => x.Id == assignment.Id);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GetAssignments_ForTeacher_OnlyReturnsOwnClassSubjects()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var otherTeacher = TestData.CreateUser(db, "Teacher", "other@school.local");

            var own = TestData.CreateClassSubject(db, "Class 9", "Mathematics");
            TestData.AssignTeacher(db, own.Id, teacher.Id);
            var other = TestData.CreateClassSubject(db, "Class 10", "Physics");
            TestData.AssignTeacher(db, other.Id, otherTeacher.Id);

            var ownAssignment = TestData.CreateAssignment(db, own.Id, teacher.Id);
            var otherAssignment = TestData.CreateAssignment(db, other.Id, otherTeacher.Id);

            var service = new AssignmentService(db, TestServices.As(teacher));

            var result = await service.GetAssignments(null);

            Assert.Contains(result, x => x.Id == ownAssignment.Id);
            Assert.DoesNotContain(result, x => x.Id == otherAssignment.Id);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GetTeacherClassSubjects_OnlyReturnsActiveClassSubjectsTeacherIsAssignedTo()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var otherTeacher = TestData.CreateUser(db, "Teacher", "other@school.local");

            var assigned = TestData.CreateClassSubject(db, "Class 9", "Mathematics");
            TestData.AssignTeacher(db, assigned.Id, teacher.Id);

            var inactive = TestData.CreateClassSubject(db, "Class 10", "Biology");
            TestData.AssignTeacher(db, inactive.Id, teacher.Id);
            inactive.IsActive = false;
            db.SaveChanges();

            var notAssigned = TestData.CreateClassSubject(db, "Class 11", "Physics");
            TestData.AssignTeacher(db, notAssigned.Id, otherTeacher.Id);

            var service = new AssignmentService(db, TestServices.As(teacher));

            var result = await service.GetTeacherClassSubjects();

            Assert.Contains(result, x => x.Id == assigned.Id);
            Assert.DoesNotContain(result, x => x.Id == inactive.Id);
            Assert.DoesNotContain(result, x => x.Id == notAssigned.Id);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task CreateAssignment_ByTeacherNotAssignedToClassSubject_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var classSubject = TestData.CreateClassSubject(db);

            var service = new AssignmentService(db, TestServices.As(teacher));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.CreateAssignment(new CreateAssignmentRequest(
                    classSubject.Id,
                    "Homework",
                    "Description",
                    100,
                    DateTime.UtcNow.AddDays(7))));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task CreateAssignment_WithPastDeadline_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);

            var service = new AssignmentService(db, TestServices.As(teacher));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAssignment(new CreateAssignmentRequest(
                    classSubject.Id,
                    "Homework",
                    "Description",
                    100,
                    DateTime.UtcNow.AddHours(-1))));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task CreateAssignment_WithStartAfterOrEqualToDeadline_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);

            var service = new AssignmentService(db, TestServices.As(teacher));

            var deadline = DateTime.UtcNow.AddDays(7);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAssignment(new CreateAssignmentRequest(
                    classSubject.Id,
                    "Homework",
                    "Description",
                    100,
                    deadline,
                    StartsAtUtc: deadline.AddHours(1))));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GetAssignmentById_ForUnassignedTeacher_ThrowsNotFound()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var otherTeacher = TestData.CreateUser(db, "Teacher", "other@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, otherTeacher.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, otherTeacher.Id);

            var service = new AssignmentService(db, TestServices.As(teacher));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.GetAssignmentById(assignment.Id));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }
}
