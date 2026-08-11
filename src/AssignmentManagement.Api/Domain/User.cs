namespace AssignmentManagement.Api.Domain;

public class User
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
