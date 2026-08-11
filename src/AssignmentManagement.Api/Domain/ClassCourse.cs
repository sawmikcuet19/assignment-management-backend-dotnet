using System.Text.Json.Serialization;

namespace AssignmentManagement.Api.Domain;

public class ClassCourse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();

    [JsonIgnore]
    public ICollection<StudentClass> StudentEnrollments { get; set; } = new List<StudentClass>();
}
