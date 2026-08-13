using AssignmentManagement.Api.Dtos;

namespace AssignmentManagement.Api.Services.Interfaces;

public interface ISubmissionService
{
    Task<SubmissionDto> Submit(int assignmentId, CreateSubmissionRequest request);
    Task<SubmissionDto> UpdateSubmission(int submissionId, UpdateSubmissionRequest request);
    Task<SubmissionDto?> GetMySubmission(int assignmentId);
    Task<List<SubmissionDto>> GetSubmissionsForAssignment(int assignmentId);
    Task<SubmissionDto> GetSubmissionById(int submissionId);
    Task<SubmissionDto> GradeSubmission(int submissionId, GradeSubmissionRequest request);
}