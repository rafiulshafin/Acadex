using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[Authorize]
public class ClassesController : BaseApiController
{
    private readonly IClassSubjectService _classSubjectService;

    public ClassesController(IClassSubjectService classSubjectService)
    {
        _classSubjectService = classSubjectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SchoolClassDto>>>> GetClasses()
    {
        var classes = await _classSubjectService.GetClassesAsync();
        return Ok(ApiResponse<List<SchoolClassDto>>.Ok(classes));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SchoolClassDto>>> GetClassById(Guid id)
    {
        var c = await _classSubjectService.GetClassByIdAsync(id);
        return Ok(ApiResponse<SchoolClassDto>.Ok(c));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SchoolClassDto>>> CreateClass([FromBody] CreateSchoolClassRequest request)
    {
        var c = await _classSubjectService.CreateClassAsync(request);
        return CreatedAtAction(nameof(GetClassById), new { id = c.Id }, ApiResponse<SchoolClassDto>.Ok(c, "Class created successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteClass(Guid id)
    {
        var result = await _classSubjectService.DeleteClassAsync(id);
        return Ok(ApiResponse<bool>.Ok(result, "Class deleted successfully"));
    }
}
