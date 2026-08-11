using System.Text.Json.Serialization;

namespace AssignmentManagement.Api.Domain;

public class Submission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public int StudentUserId { get; set; }
    public User? Student { get; set; }

    public string AnswerText { get; set; } = default!;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    public int? MarksObtained { get; set; }
    public string? Feedback { get; set; }

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    [JsonIgnore]
    public ICollection<SubmissionAttachment> Attachments { get; set; } = new List<SubmissionAttachment>();
}
