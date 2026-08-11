using AssignmentManagement.Api.Domain;

namespace AssignmentManagement.Api.Dtos;

public record CreateAssignmentRequest(
    int ClassSubjectId,
    string Title,
    string Description,
    int MaxMarks,
    DateTime DeadlineUtc,
    DateTime? StartsAtUtc = null,
    bool AllowUpdateBeforeDeadline = true
);

public record UpdateAssignmentRequest(
    string Title,
    string Description,
    int MaxMarks,
    DateTime DeadlineUtc,
    DateTime? StartsAtUtc = null,
    bool AllowUpdateBeforeDeadline = true
);

public record AssignmentListItemDto(
    int Id,
    int ClassSubjectId,
    string Title,
    AssignmentStatus Status,
    int MaxMarks,
    DateTime DeadlineUtc,
    DateTime? StartsAtUtc,
    string ClassName,
    string SubjectName
);

public record AssignmentDetailsDto(
    int Id,
    int ClassSubjectId,
    string Title,
    string Description,
    int MaxMarks,
    DateTime DeadlineUtc,
    DateTime? StartsAtUtc,
    AssignmentStatus Status,
    bool AllowUpdateBeforeDeadline,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string ClassName,
    string SubjectName
);
