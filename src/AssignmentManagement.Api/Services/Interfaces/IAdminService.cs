using AssignmentManagement.Api.Dtos;

namespace AssignmentManagement.Api.Services.Interfaces;

public interface IAdminService
{
    Task<List<UserResponse>> GetUsers();
    Task<UserResponse> CreateUser(CreateUserRequest request);
    Task<UserResponse> UpdateUser(int id, UpdateUserRequest request);
    Task DeactivateUser(int id);
    Task HardDeleteUser(int id);
    Task ActivateUser(int id);
    Task<List<ClassResponse>> GetClasses();
    Task<ClassResponse> CreateClass(CreateClassRequest request);
    Task<ClassResponse> UpdateClass(int id, UpdateClassRequest request);
    Task DeactivateClass(int id);
    Task HardDeleteClass(int id);
    Task ActivateClass(int id);
    Task<List<SubjectResponse>> GetSubjects();
    Task<SubjectResponse> CreateSubject(CreateSubjectRequest request);
    Task<SubjectResponse> UpdateSubject(int id, UpdateSubjectRequest request);
    Task DeactivateSubject(int id);
    Task HardDeleteSubject(int id);
    Task<List<ClassSubjectDetailDto>> GetClassSubjects();
    Task<ClassSubjectDetailDto> CreateClassSubject(CreateClassSubjectRequest request);
    Task<ClassSubjectDetailDto> UpdateClassSubject(int id, UpdateClassSubjectRequest request);
    Task DeactivateClassSubject(int id);
    Task ActivateSubject(int id);
    Task AssignTeacher(int classSubjectId, int teacherUserId);
    Task RemoveTeacher(int classSubjectId, int teacherUserId);
    Task<List<UserResponse>> GetClassSubjectTeachers(int classSubjectId);
    Task EnrollStudent(int classId, int studentUserId);
    Task RemoveStudent(int classId, int studentUserId);
    Task<List<UserResponse>> GetClassStudents(int classId);
}