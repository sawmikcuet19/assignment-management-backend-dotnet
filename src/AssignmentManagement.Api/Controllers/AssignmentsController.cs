using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        this.assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignments([FromQuery] AssignmentStatus? status)
    {
        return Ok(await assignmentService.GetAssignments(status));
    }

    [HttpGet("class-subjects")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetTeacherClassSubjects()
    {
        return Ok(await assignmentService.GetTeacherClassSubjects());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAssignment(int id)
    {
        return Ok(await assignmentService.GetAssignmentById(id));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateAssignment(CreateAssignmentRequest request)
    {
        var result = await assignmentService.CreateAssignment(request);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UpdateAssignment(int id, UpdateAssignmentRequest request)
    {
        return Ok(await assignmentService.UpdateAssignment(id, request));
    }

    [HttpPost("{id:int}/publish")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> PublishAssignment(int id)
    {
        await assignmentService.PublishAssignment(id);
        return Ok(new
        {
            message = "Assignment published."
        });
    }

    [HttpPost("{id:int}/archive")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> ArchiveAssignment(int id)
    {
        await assignmentService.ArchiveAssignment(id);
        return Ok(new
        {
            message = "Assignment archived."
        });
    }

    [HttpPost("{id:int}/unarchive")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UnarchiveAssignment(int id)
    {
        await assignmentService.UnarchiveAssignment(id);
        return Ok(new
        {
            message = "Assignment unarchived."
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var message = await assignmentService.DeleteAssignment(id);
        return Ok(new
        {
            message
        });
    }
}