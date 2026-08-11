using AssignmentManagement.Api.Auth;
using AssignmentManagement.Api.Data;
using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Api.Services;

public class AuthService
{
    private readonly AppDbContext db;
    private readonly JwtTokenService jwtTokenService;

    public AuthService(AppDbContext db, JwtTokenService jwtTokenService)
    {
        this.db = db;
        this.jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var user = await db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive);

        if (user is null)
        {
            return null;
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return null;
        }

        var token = jwtTokenService.Generate(user);

        return new LoginResponse(
            Token: token,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role?.Name ?? "Student"
        );
    }

    public async Task<LoginResponse> Register(RegisterRequest request)
    {
        var emailExists = await db.Users
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var studentRole = await db.Roles
            .FirstOrDefaultAsync(x => x.Name == "Student")
            ?? throw new InvalidOperationException("The 'Student' role is not configured.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = studentRole.Id,
            Role = studentRole,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = jwtTokenService.Generate(user);

        return new LoginResponse(
            Token: token,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role?.Name ?? "Student"
        );
    }
}