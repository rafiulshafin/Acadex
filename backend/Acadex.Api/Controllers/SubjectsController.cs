using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[Authorize]
public class SubjectsController : BaseApiController
{
    private readonly IClassSubjectService _classSubjectService;

    public SubjectsController(IClassSubjectService classSubjectService)
    {
        _classSubjectService = classSubjectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SubjectDto>>>> GetSubjects()
    {
        var subjects = await _classSubjectService.GetSubjectsAsync();
        return Ok(ApiResponse<List<SubjectDto>>.Ok(subjects));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> GetSubjectById(Guid id)
    {
        var s = await _classSubjectService.GetSubjectByIdAsync(id);
        return Ok(ApiResponse<SubjectDto>.Ok(s));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> CreateSubject([FromBody] CreateSubjectRequest request)
    {
        var s = await _classSubjectService.CreateSubjectAsync(request);
        return CreatedAtAction(nameof(GetSubjectById), new { id = s.Id }, ApiResponse<SubjectDto>.Ok(s, "Subject created successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSubject(Guid id)
    {
        var result = await _classSubjectService.DeleteSubjectAsync(id);
        return Ok(ApiResponse<bool>.Ok(result, "Subject deleted successfully"));
    }
}
