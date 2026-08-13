using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;

namespace AssignmentManagement.Api.Services.Interfaces;

public interface IAssignmentService
{
    Task<List<AssignmentListItemDto>> GetAssignments(AssignmentStatus? status);
    Task<List<ClassSubjectDetailDto>> GetTeacherClassSubjects();
    Task<AssignmentDetailsDto> GetAssignmentById(int id);
    Task<AssignmentDetailsDto> CreateAssignment(CreateAssignmentRequest request);
    Task<AssignmentDetailsDto> UpdateAssignment(int id, UpdateAssignmentRequest request);
    Task PublishAssignment(int id);
    Task ArchiveAssignment(int id);
    Task UnarchiveAssignment(int id);
    Task<string> DeleteAssignment(int id);
}