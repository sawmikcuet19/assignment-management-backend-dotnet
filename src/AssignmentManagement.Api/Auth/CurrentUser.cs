using System.Security.Claims;

namespace AssignmentManagement.Api.Auth;

public class CurrentUser
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public CurrentUser()
    {
    }

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user is null)
        {
            return;
        }

        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        var emailClaim = user.FindFirst(ClaimTypes.Email)?.Value;

        if (int.TryParse(idClaim, out var userId))
        {
            Id = userId;
        }

        Role = roleClaim ?? string.Empty;
        Email = emailClaim ?? string.Empty;
    }

    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsTeacher => Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase);
    public bool IsStudent => Role.Equals("Student", StringComparison.OrdinalIgnoreCase);
    public bool IsAuthenticated => Id > 0;
}