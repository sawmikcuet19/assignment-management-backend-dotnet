using System.Text.Json.Serialization;

namespace AssignmentManagement.Api.Domain;

public class Assignment
{
    public int Id { get; set; }

    public int ClassSubjectId { get; set; }
    public ClassSubject? ClassSubject { get; set; }

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    public int MaxMarks { get; set; }
    public DateTime DeadlineUtc { get; set; }
    public DateTime? StartsAtUtc { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    public bool AllowUpdateBeforeDeadline { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
