using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService submissionService;

    public SubmissionsController(ISubmissionService submissionService)
    {
        this.submissionService = submissionService;
    }

    [HttpPost("assignments/{assignmentId:int}/submissions")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit(int assignmentId, CreateSubmissionRequest request)
    {
        var result = await submissionService.Submit(assignmentId, request);
        return Ok(result);
    }

    [HttpGet("assignments/{assignmentId:int}/submissions/me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySubmission(int assignmentId)
    {
        var result = await submissionService.GetMySubmission(assignmentId);

        if (result is null)
        {
            return NotFound(new
            {
                message = "No submission found."
            });
        }

        return Ok(result);
    }

    [HttpGet("assignments/{assignmentId:int}/submissions")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetSubmissionsForAssignment(int assignmentId)
    {
        return Ok(await submissionService.GetSubmissionsForAssignment(assignmentId));
    }

    [HttpGet("submissions/{id:int}")]
    public async Task<IActionResult> GetSubmission(int id)
    {
        return Ok(await submissionService.GetSubmissionById(id));
    }

    [HttpPut("submissions/{id:int}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UpdateSubmission(int id, UpdateSubmissionRequest request)
    {
        return Ok(await submissionService.UpdateSubmission(id, request));
    }

    [HttpPut("submissions/{id:int}/grade")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GradeSubmission(int id, GradeSubmissionRequest request)
    {
        return Ok(await submissionService.GradeSubmission(id, request));
    }
}