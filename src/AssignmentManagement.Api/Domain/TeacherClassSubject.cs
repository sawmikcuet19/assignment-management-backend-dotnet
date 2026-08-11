namespace AssignmentManagement.Api.Domain;

public class TeacherClassSubject
{
    public int Id { get; set; }

    public int ClassSubjectId { get; set; }
    public ClassSubject? ClassSubject { get; set; }

    public int TeacherUserId { get; set; }
    public User? Teacher { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
