using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services;
using Microsoft.Data.Sqlite;

namespace AssignmentManagement.Tests;

public class SubmissionServiceTests
{
    [Fact]
    public async Task Submit_ByStudentNotEnrolled_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var service = new SubmissionService(db, TestServices.As(student));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.Submit(assignment.Id, new CreateSubmissionRequest("My answer")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Submit_ForDraftAssignment_ThrowsNotFound()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id,
                status: AssignmentStatus.Draft);

            var service = new SubmissionService(db, TestServices.As(student));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.Submit(assignment.Id, new CreateSubmissionRequest("My answer")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Submit_AfterDeadline_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id,
                deadlineUtc: DateTime.UtcNow.AddHours(-1));

            var service = new SubmissionService(db, TestServices.As(student));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.Submit(assignment.Id, new CreateSubmissionRequest("My answer")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Submit_Twice_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var service = new SubmissionService(db, TestServices.As(student));

            await service.Submit(assignment.Id, new CreateSubmissionRequest("First answer"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.Submit(assignment.Id, new CreateSubmissionRequest("Second answer")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task UpdateSubmission_WhenUpdatesNotAllowed_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id,
                allowUpdateBeforeDeadline: false);

            var service = new SubmissionService(db, TestServices.As(student));
            var submission = await service.Submit(assignment.Id, new CreateSubmissionRequest("Answer"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateSubmission(submission.Id, new UpdateSubmissionRequest("Updated answer")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task UpdateSubmission_AfterDeadline_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);

            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id,
                deadlineUtc: DateTime.UtcNow.AddHours(1));
            var service = new SubmissionService(db, TestServices.As(student));
            var submission = await service.Submit(assignment.Id, new CreateSubmissionRequest("Answer"));

            assignment.DeadlineUtc = DateTime.UtcNow.AddHours(-1);
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateSubmission(submission.Id, new UpdateSubmissionRequest("Updated answer")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GradeSubmission_WithMarksAboveMaximum_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id, maxMarks: 50);

            var submissionService = new SubmissionService(db, TestServices.As(student));
            var submission = await submissionService.Submit(assignment.Id, new CreateSubmissionRequest("Answer"));

            var service = new SubmissionService(db, TestServices.As(teacher));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GradeSubmission(submission.Id, new GradeSubmissionRequest(51, "Good", SubmissionStatus.Graded)));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GradeSubmission_WithNegativeMarks_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var submissionService = new SubmissionService(db, TestServices.As(student));
            var submission = await submissionService.Submit(assignment.Id, new CreateSubmissionRequest("Answer"));

            var service = new SubmissionService(db, TestServices.As(teacher));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GradeSubmission(submission.Id, new GradeSubmissionRequest(-1, null, SubmissionStatus.Graded)));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GradeSubmission_ByTeacherNotAssigned_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var otherTeacher = TestData.CreateUser(db, "Teacher", "other@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var submissionService = new SubmissionService(db, TestServices.As(student));
            var submission = await submissionService.Submit(assignment.Id, new CreateSubmissionRequest("Answer"));

            var service = new SubmissionService(db, TestServices.As(otherTeacher));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GradeSubmission(submission.Id, new GradeSubmissionRequest(80, null, SubmissionStatus.Graded)));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GradeSubmission_ByAdmin_Succeeds()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var admin = TestData.CreateUser(db, "Admin", "admin@school.local");
            var student = TestData.CreateUser(db, "Student", "student@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            TestData.EnrollStudent(db, classSubject.ClassId, student.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var submissionService = new SubmissionService(db, TestServices.As(student));
            var submission = await submissionService.Submit(assignment.Id, new CreateSubmissionRequest("Answer"));

            var service = new SubmissionService(db, TestServices.As(admin));

            var graded = await service.GradeSubmission(
                submission.Id,
                new GradeSubmissionRequest(90, "Well done", SubmissionStatus.Graded));

            Assert.Equal(90, graded.MarksObtained);
            Assert.Equal("Well done", graded.Feedback);
            Assert.Equal(SubmissionStatus.Graded, graded.Status);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task GetSubmissionsForAssignment_ByUnassignedTeacher_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var teacher = TestData.CreateUser(db, "Teacher", "teacher@school.local");
            var otherTeacher = TestData.CreateUser(db, "Teacher", "other@school.local");
            var classSubject = TestData.CreateClassSubject(db);
            TestData.AssignTeacher(db, classSubject.Id, teacher.Id);
            var assignment = TestData.CreateAssignment(db, classSubject.Id, teacher.Id);

            var service = new SubmissionService(db, TestServices.As(otherTeacher));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetSubmissionsForAssignment(assignment.Id));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }
}
