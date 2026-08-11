using System.Text.Json.Serialization;

namespace AssignmentManagement.Api.Domain;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
}
