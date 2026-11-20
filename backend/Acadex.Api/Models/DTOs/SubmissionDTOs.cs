using System.ComponentModel.DataAnnotations;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Models.DTOs;

public class CreateSubmissionRequest
{
    [Required]
    public Guid AssignmentId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}

public class UpdateSubmissionRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}

public class GradeSubmissionRequest
{
    [Required]
    [Range(0, 1000)]
    public decimal MarksAwarded { get; set; }

    public string? Feedback { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Graded;
}

public class UpdateSubmissionStatusRequest
{
    [Required]
    public SubmissionStatus Status { get; set; }
}

public class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public decimal AssignmentMaxMarks { get; set; }
    public DateTime AssignmentDeadline { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public SubmissionStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsLate { get; set; }
    public decimal? MarksAwarded { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? GradedByName { get; set; }
}
