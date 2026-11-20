using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminDashboardStatsDto>>> GetAdminDashboard()
    {
        var stats = await _dashboardService.GetAdminStatsAsync();
        return Ok(ApiResponse<AdminDashboardStatsDto>.Ok(stats));
    }

    [HttpGet("teacher")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<TeacherDashboardStatsDto>>> GetTeacherDashboard()
    {
        var stats = await _dashboardService.GetTeacherStatsAsync(CurrentUserId);
        return Ok(ApiResponse<TeacherDashboardStatsDto>.Ok(stats));
    }

    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponse<StudentDashboardStatsDto>>> GetStudentDashboard()
    {
        var stats = await _dashboardService.GetStudentStatsAsync(CurrentUserId);
        return Ok(ApiResponse<StudentDashboardStatsDto>.Ok(stats));
    }
}
