using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[Authorize]
public class SubmissionsController : BaseApiController
{
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SubmissionDto>>>> GetSubmissions(
        [FromQuery] Guid? assignmentId,
        [FromQuery] SubmissionStatus? status)
    {
        var submissions = await _submissionService.GetSubmissionsAsync(CurrentUserId, CurrentUserRole, assignmentId, status);
        return Ok(ApiResponse<List<SubmissionDto>>.Ok(submissions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> GetSubmissionById(Guid id)
    {
        var submission = await _submissionService.GetSubmissionByIdAsync(id, CurrentUserId, CurrentUserRole);
        return Ok(ApiResponse<SubmissionDto>.Ok(submission));
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> SubmitAssignment([FromBody] CreateSubmissionRequest request)
    {
        var submission = await _submissionService.SubmitAssignmentAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetSubmissionById), new { id = submission.Id }, ApiResponse<SubmissionDto>.Ok(submission, "Assignment submitted successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> UpdateSubmission(Guid id, [FromBody] UpdateSubmissionRequest request)
    {
        var submission = await _submissionService.UpdateSubmissionAsync(id, request, CurrentUserId);
        return Ok(ApiResponse<SubmissionDto>.Ok(submission, "Submission updated successfully"));
    }

    [HttpPost("{id}/grade")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> GradeSubmission(Guid id, [FromBody] GradeSubmissionRequest request)
    {
        var submission = await _submissionService.GradeSubmissionAsync(id, request, CurrentUserId, IsAdmin);
        return Ok(ApiResponse<SubmissionDto>.Ok(submission, "Submission graded successfully"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> UpdateSubmissionStatus(Guid id, [FromBody] UpdateSubmissionStatusRequest request)
    {
        var submission = await _submissionService.UpdateSubmissionStatusAsync(id, request.Status, CurrentUserId, IsAdmin);
        return Ok(ApiResponse<SubmissionDto>.Ok(submission, "Submission status updated successfully"));
    }
}
