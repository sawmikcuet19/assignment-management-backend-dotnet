using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService adminService;

    public AdminController(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await adminService.GetUsers());
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await adminService.CreateUser(request);
        return Ok(result);
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        return Ok(await adminService.UpdateUser(id, request));
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        await adminService.DeactivateUser(id);
        return Ok(new { message = "User deactivated successfully." });
    }

    [HttpDelete("users/{id:int}/hard-delete")]
    public async Task<IActionResult> HardDeleteUser(int id)
    {
        await adminService.HardDeleteUser(id);
        return Ok(new { message = "User permanently deleted." });
    }

    [HttpPost("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        await adminService.ActivateUser(id);
        return Ok(new { message = "User activated successfully." });
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        return Ok(await adminService.GetClasses());
    }

    [HttpPost("classes")]
    public async Task<IActionResult> CreateClass(CreateClassRequest request)
    {
        return Ok(await adminService.CreateClass(request));
    }

    [HttpPut("classes/{id:int}")]
    public async Task<IActionResult> UpdateClass(int id, UpdateClassRequest request)
    {
        return Ok(await adminService.UpdateClass(id, request));
    }

    [HttpDelete("classes/{id:int}")]
    public async Task<IActionResult> DeactivateClass(int id)
    {
        await adminService.DeactivateClass(id);
        return Ok(new { message = "Class subject deactivated successfully." });
    }

    [HttpDelete("classes/{id:int}/hard-delete")]
    public async Task<IActionResult> HardDeleteClass(int id)
    {
        await adminService.HardDeleteClass(id);
        return Ok(new { message = "Class subject permanently deleted." });
    }

    [HttpPost("classes/{id:int}/activate")]
    public async Task<IActionResult> ActivateClass(int id)
    {
        await adminService.ActivateClass(id);
        return Ok(new { message = "Class subject activated successfully." });
    }

    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        return Ok(await adminService.GetSubjects());
    }

    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject(CreateSubjectRequest request)
    {
        return Ok(await adminService.CreateSubject(request));
    }

    [HttpPut("subjects/{id:int}")]
    public async Task<IActionResult> UpdateSubject(int id, UpdateSubjectRequest request)
    {
        return Ok(await adminService.UpdateSubject(id, request));
    }

    [HttpDelete("subjects/{id:int}")]
    public async Task<IActionResult> DeactivateSubject(int id)
    {
        await adminService.DeactivateSubject(id);
        return Ok(new { message = "Subject deactivated successfully." });
    }

    [HttpDelete("subjects/{id:int}/hard-delete")]
    public async Task<IActionResult> HardDeleteSubject(int id)
    {
        await adminService.HardDeleteSubject(id);
        return Ok(new { message = "Subject permanently deleted." });
    }

    [HttpPost("subjects/{id:int}/activate")]
    public async Task<IActionResult> ActivateSubject(int id)
    {
        await adminService.ActivateSubject(id);
        return Ok(new { message = "Subject activated successfully." });
    }

    [HttpGet("class-subjects")]
    public async Task<IActionResult> GetClassSubjects()
    {
        return Ok(await adminService.GetClassSubjects());
    }

    [HttpPost("class-subjects")]
    public async Task<IActionResult> CreateClassSubject(CreateClassSubjectRequest request)
    {
        return Ok(await adminService.CreateClassSubject(request));
    }

    [HttpPut("class-subjects/{id:int}")]
    public async Task<IActionResult> UpdateClassSubject(int id, UpdateClassSubjectRequest request)
    {
        return Ok(await adminService.UpdateClassSubject(id, request));
    }

    [HttpDelete("class-subjects/{id:int}")]
    public async Task<IActionResult> DeactivateClassSubject(int id)
    {
        await adminService.DeactivateClassSubject(id);
        return Ok(new { message = "Class subject deactivated successfully." });
    }

    [HttpPost("class-subjects/{classSubjectId:int}/teachers")]
    public async Task<IActionResult> AssignTeacher(int classSubjectId, AssignTeacherRequest request)
    {
        await adminService.AssignTeacher(classSubjectId, request.TeacherUserId);
        return Ok(new
        {
            message = "Teacher assigned successfully."
        });
    }

    [HttpDelete("class-subjects/{classSubjectId:int}/teachers/{teacherUserId:int}")]
    public async Task<IActionResult> RemoveTeacher(int classSubjectId, int teacherUserId)
    {
        await adminService.RemoveTeacher(classSubjectId, teacherUserId);
        return Ok(new { message = "Teacher removed successfully." });
    }

    [HttpGet("class-subjects/{classSubjectId:int}/teachers")]
    public async Task<IActionResult> GetClassSubjectTeachers(int classSubjectId)
    {
        return Ok(await adminService.GetClassSubjectTeachers(classSubjectId));
    }

    [HttpPost("classes/{classId:int}/students")]
    public async Task<IActionResult> EnrollStudent(int classId, EnrollStudentRequest request)
    {
        await adminService.EnrollStudent(classId, request.StudentUserId);
        return Ok(new
        {
            message = "Student enrolled successfully."
        });
    }

    [HttpDelete("classes/{classId:int}/students/{studentUserId:int}")]
    public async Task<IActionResult> RemoveStudent(int classId, int studentUserId)
    {
        await adminService.RemoveStudent(classId, studentUserId);
        return Ok(new { message = "Student removed successfully." });
    }

    [HttpGet("classes/{classId:int}/students")]
    public async Task<IActionResult> GetClassStudents(int classId)
    {
        return Ok(await adminService.GetClassStudents(classId));
    }
}