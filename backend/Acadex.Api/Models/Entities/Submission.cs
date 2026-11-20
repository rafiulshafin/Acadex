using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Models.Entities;

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    [Required]
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? MarksAwarded { get; set; }

    public string? Feedback { get; set; }

    public DateTime? GradedAt { get; set; }

    public Guid? GradedById { get; set; }
    public User? GradedBy { get; set; }
}
