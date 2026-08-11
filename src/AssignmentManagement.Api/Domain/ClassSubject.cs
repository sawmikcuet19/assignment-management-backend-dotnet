using System.Text.Json.Serialization;

namespace AssignmentManagement.Api.Domain;

public class ClassSubject
{
    public int Id { get; set; }

    public int ClassId { get; set; }
    public ClassCourse? Class { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public string? AcademicYear { get; set; }
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<TeacherClassSubject> TeacherAssignments { get; set; } = new List<TeacherClassSubject>();

    [JsonIgnore]
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
