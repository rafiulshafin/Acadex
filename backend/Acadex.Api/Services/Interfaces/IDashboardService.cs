using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardStatsDto> GetAdminStatsAsync();
    Task<TeacherDashboardStatsDto> GetTeacherStatsAsync(Guid teacherId);
    Task<StudentDashboardStatsDto> GetStudentStatsAsync(Guid studentId);
}
