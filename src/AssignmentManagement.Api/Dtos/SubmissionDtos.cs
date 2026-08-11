using AssignmentManagement.Api.Domain;

namespace AssignmentManagement.Api.Dtos;

public record CreateSubmissionRequest(string AnswerText);

public record UpdateSubmissionRequest(string AnswerText);

public record SubmissionDto(
    int Id,
    int AssignmentId,
    int StudentUserId,
    string StudentFullName,
    string AnswerText,
    SubmissionStatus Status,
    int? MarksObtained,
    string? Feedback,
    DateTime SubmittedAtUtc,
    DateTime UpdatedAtUtc
);

public record GradeSubmissionRequest(
    int MarksObtained,
    string? Feedback,
    SubmissionStatus Status
);
