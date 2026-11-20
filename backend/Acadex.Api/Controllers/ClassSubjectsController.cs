using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[Authorize]
public class ClassSubjectsController : BaseApiController
{
    private readonly IClassSubjectService _classSubjectService;

    public ClassSubjectsController(IClassSubjectService classSubjectService)
    {
        _classSubjectService = classSubjectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ClassSubjectDto>>>> GetAllocations([FromQuery] Guid? teacherId, [FromQuery] Guid? classId)
    {
        var allocations = await _classSubjectService.GetAllocationsAsync(teacherId, classId);
        return Ok(ApiResponse<List<ClassSubjectDto>>.Ok(allocations));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ClassSubjectDto>>> AssignTeacher([FromBody] AssignTeacherRequest request)
    {
        var allocation = await _classSubjectService.AssignTeacherAsync(request);
        return Ok(ApiResponse<ClassSubjectDto>.Ok(allocation, "Teacher assigned successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveAllocation(Guid id)
    {
        var result = await _classSubjectService.RemoveAllocationAsync(id);
        return Ok(ApiResponse<bool>.Ok(result, "Allocation removed successfully"));
    }
}
