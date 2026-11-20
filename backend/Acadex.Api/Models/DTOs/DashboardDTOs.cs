namespace Acadex.Api.Models.DTOs;

public class AdminDashboardStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSubjects { get; set; }
    public int TotalAssignments { get; set; }
    public int TotalSubmissions { get; set; }
    public int PendingReviewCount { get; set; }
}

public class TeacherDashboardStatsDto
{
    public int AssignedClassesCount { get; set; }
    public int AssignedSubjectsCount { get; set; }
    public int TotalAssignmentsCreated { get; set; }
    public int TotalSubmissionsReceived { get; set; }
    public int PendingGradingCount { get; set; }
}

public class StudentDashboardStatsDto
{
    public int TotalEnrolledAssignments { get; set; }
    public int SubmittedCount { get; set; }
    public int PendingCount { get; set; }
    public int GradedCount { get; set; }
    public decimal AverageScorePercentage { get; set; }
}
