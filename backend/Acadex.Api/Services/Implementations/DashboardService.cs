using Acadex.Api.Data;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AcadexDbContext _context;

    public DashboardService(AcadexDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardStatsDto> GetAdminStatsAsync()
    {
        var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student && u.IsActive);
        var totalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher && u.IsActive);
        var totalClasses = await _context.SchoolClasses.CountAsync();
        var totalSubjects = await _context.Subjects.CountAsync();
        var totalAssignments = await _context.Assignments.CountAsync();
        var totalSubmissions = await _context.Submissions.CountAsync();
        var pendingReviewCount = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview);

        return new AdminDashboardStatsDto
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalClasses = totalClasses,
            TotalSubjects = totalSubjects,
            TotalAssignments = totalAssignments,
            TotalSubmissions = totalSubmissions,
            PendingReviewCount = pendingReviewCount
        };
    }

    public async Task<TeacherDashboardStatsDto> GetTeacherStatsAsync(Guid teacherId)
    {
        var allocations = await _context.ClassSubjects
            .Where(cs => cs.TeacherId == teacherId)
            .ToListAsync();

        var assignedClassesCount = allocations.Select(cs => cs.SchoolClassId).Distinct().Count();
        var assignedSubjectsCount = allocations.Select(cs => cs.SubjectId).Distinct().Count();

        var totalAssignmentsCreated = await _context.Assignments.CountAsync(a => a.TeacherId == teacherId);

        var submissionsQuery = _context.Submissions.Where(s => s.Assignment.TeacherId == teacherId);
        var totalSubmissionsReceived = await submissionsQuery.CountAsync();
        var pendingGradingCount = await submissionsQuery.CountAsync(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview);

        return new TeacherDashboardStatsDto
        {
            AssignedClassesCount = assignedClassesCount,
            AssignedSubjectsCount = assignedSubjectsCount,
            TotalAssignmentsCreated = totalAssignmentsCreated,
            TotalSubmissionsReceived = totalSubmissionsReceived,
            PendingGradingCount = pendingGradingCount
        };
    }

    public async Task<StudentDashboardStatsDto> GetStudentStatsAsync(Guid studentId)
    {
        var student = await _context.Users.FindAsync(studentId);
        if (student == null || !student.SchoolClassId.HasValue)
        {
            return new StudentDashboardStatsDto();
        }

        var totalEnrolledAssignments = await _context.Assignments
            .CountAsync(a => a.SchoolClassId == student.SchoolClassId.Value && a.Status == AssignmentStatus.Published);

        var submissions = await _context.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == studentId)
            .ToListAsync();

        var submittedCount = submissions.Count;
        var pendingCount = Math.Max(0, totalEnrolledAssignments - submittedCount);
        var gradedList = submissions.Where(s => s.Status == SubmissionStatus.Graded && s.MarksAwarded.HasValue).ToList();
        var gradedCount = gradedList.Count;

        decimal averageScorePercentage = 0;
        if (gradedCount > 0)
        {
            var percentages = gradedList
                .Where(s => s.Assignment.MaxMarks > 0)
                .Select(s => (s.MarksAwarded!.Value / s.Assignment.MaxMarks) * 100);

            if (percentages.Any())
            {
                averageScorePercentage = Math.Round(percentages.Average(), 2);
            }
        }

        return new StudentDashboardStatsDto
        {
            TotalEnrolledAssignments = totalEnrolledAssignments,
            SubmittedCount = submittedCount,
            PendingCount = pendingCount,
            GradedCount = gradedCount,
            AverageScorePercentage = averageScorePercentage
        };
    }
}
