using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[Authorize]
public class AssignmentsController : BaseApiController
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AssignmentDto>>>> GetAssignments(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? subjectId,
        [FromQuery] AssignmentStatus? status)
    {
        var assignments = await _assignmentService.GetAssignmentsAsync(CurrentUserId, CurrentUserRole, classId, subjectId, status);
        return Ok(ApiResponse<List<AssignmentDto>>.Ok(assignments));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> GetAssignmentById(Guid id)
    {
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id, CurrentUserId, CurrentUserRole);
        return Ok(ApiResponse<AssignmentDto>.Ok(assignment));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> CreateAssignment([FromBody] CreateAssignmentRequest request)
    {
        var assignment = await _assignmentService.CreateAssignmentAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetAssignmentById), new { id = assignment.Id }, ApiResponse<AssignmentDto>.Ok(assignment, "Assignment created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> UpdateAssignment(Guid id, [FromBody] UpdateAssignmentRequest request)
    {
        var assignment = await _assignmentService.UpdateAssignmentAsync(id, request, CurrentUserId, IsAdmin);
        return Ok(ApiResponse<AssignmentDto>.Ok(assignment, "Assignment updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAssignment(Guid id)
    {
        var result = await _assignmentService.DeleteAssignmentAsync(id, CurrentUserId, IsAdmin);
        return Ok(ApiResponse<bool>.Ok(result, "Assignment deleted successfully"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> ChangeStatus(Guid id, [FromQuery] AssignmentStatus status)
    {
        var assignment = await _assignmentService.ChangeAssignmentStatusAsync(id, status, CurrentUserId, IsAdmin);
        return Ok(ApiResponse<AssignmentDto>.Ok(assignment, "Assignment status updated successfully"));
    }
}
