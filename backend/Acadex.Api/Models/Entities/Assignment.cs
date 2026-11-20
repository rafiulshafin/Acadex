using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Models.Entities;

public class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid SchoolClassId { get; set; }
    public SchoolClass SchoolClass { get; set; } = null!;

    [Required]
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    [Required]
    public Guid TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public DateTime Deadline { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal MaxMarks { get; set; } = 100.00m;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public bool AllowLateSubmissions { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
