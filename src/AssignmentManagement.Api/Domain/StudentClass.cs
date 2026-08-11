namespace AssignmentManagement.Api.Domain;

public class StudentClass
{
    public int Id { get; set; }

    public int ClassId { get; set; }
    public ClassCourse? Class { get; set; }

    public int StudentUserId { get; set; }
    public User? Student { get; set; }

    public string? AcademicYear { get; set; }
    public bool IsActive { get; set; } = true;
}
