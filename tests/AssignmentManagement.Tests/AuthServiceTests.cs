using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services;
using Microsoft.Data.Sqlite;

namespace AssignmentManagement.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_CreatesUserWithStudentRoleByDefault()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var service = new AuthService(db, TestServices.CreateJwtTokenService());

            var result = await service.Register(new RegisterRequest(
                "Jane Doe",
                "jane@school.local",
                "password123"));

            Assert.Equal("Student", result.Role);
            Assert.Equal("Jane Doe", result.FullName);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            var user = db.Users.Single(x => x.Email == "jane@school.local");
            Assert.Equal("Student", user.Role!.Name);
            Assert.True(user.IsActive);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Register_DuplicateEmail_Throws()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            TestData.CreateUser(db, "Student", "existing@school.local");
            var service = new AuthService(db, TestServices.CreateJwtTokenService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.Register(new RegisterRequest("Jane", "existing@school.local", "password123")));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsTokenAndRole()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            TestData.CreateUser(db, "Teacher", "teacher@school.local", "Demo Teacher");
            var service = new AuthService(db, TestServices.CreateJwtTokenService());

            var result = await service.Login(new LoginRequest("teacher@school.local", "password"));

            Assert.NotNull(result);
            Assert.Equal("Teacher", result!.Role);
            Assert.Equal("Demo Teacher", result.FullName);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNull()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            TestData.CreateUser(db, "Student", "student@school.local");
            var service = new AuthService(db, TestServices.CreateJwtTokenService());

            var result = await service.Login(new LoginRequest("student@school.local", "wrong-password"));

            Assert.Null(result);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }

    [Fact]
    public async Task Login_ForInactiveUser_ReturnsNull()
    {
        var (db, connection) = TestDb.Create();
        try
        {
            var user = TestData.CreateUser(db, "Student", "inactive@school.local");
            user.IsActive = false;
            db.SaveChanges();

            var service = new AuthService(db, TestServices.CreateJwtTokenService());

            var result = await service.Login(new LoginRequest("inactive@school.local", "password"));

            Assert.Null(result);
        }
        finally
        {
            TestData.Dispose(db, connection);
        }
    }
}
