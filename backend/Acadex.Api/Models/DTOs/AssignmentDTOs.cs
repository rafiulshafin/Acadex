using System.ComponentModel.DataAnnotations;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Models.DTOs;

public class CreateAssignmentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid SchoolClassId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Range(1, 1000)]
    public decimal MaxMarks { get; set; } = 100.00m;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public bool AllowLateSubmissions { get; set; } = false;
}

public class UpdateAssignmentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Deadline { get; set; }

    [Range(1, 1000)]
    public decimal MaxMarks { get; set; } = 100.00m;

    public AssignmentStatus Status { get; set; }

    public bool AllowLateSubmissions { get; set; }
}

public class AssignmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid SchoolClassId { get; set; }
    public string SchoolClassName { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public decimal MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool AllowLateSubmissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Summary counters
    public int TotalSubmissions { get; set; }
    public int GradedSubmissions { get; set; }

    // Student specific context (if queried by student)
    public bool HasSubmitted { get; set; }
    public SubmissionDto? MySubmission { get; set; }
}
