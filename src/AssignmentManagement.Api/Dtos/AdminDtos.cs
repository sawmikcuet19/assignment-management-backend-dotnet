namespace AssignmentManagement.Api.Dtos;

public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string Role
);

public record UpdateUserRequest(
    string FullName,
    string Email,
    string? Password,
    string Role,
    bool IsActive
);

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive
);

public record CreateClassRequest(
    string Name,
    string? Code,
    string? Description
);

public record UpdateClassRequest(
    string Name,
    string? Code,
    string? Description,
    bool IsActive
);

public record ClassResponse(
    int Id,
    string Name,
    string? Code,
    string? Description,
    bool IsActive
);

public record CreateSubjectRequest(
    string Name,
    string? Code,
    string? Description
);

public record UpdateSubjectRequest(
    string Name,
    string? Code,
    string? Description,
    bool IsActive
);

public record SubjectResponse(
    int Id,
    string Name,
    string? Code,
    string? Description,
    bool IsActive
);

public record CreateClassSubjectRequest(
    int ClassId,
    int SubjectId,
    string? AcademicYear
);

public record UpdateClassSubjectRequest(
    string? AcademicYear,
    bool IsActive
);

public record ClassSubjectDetailDto(
    int Id,
    int ClassId,
    string ClassName,
    int SubjectId,
    string SubjectName,
    string? AcademicYear,
    bool IsActive
);

public record AssignTeacherRequest(int TeacherUserId);

public record EnrollStudentRequest(int StudentUserId);
