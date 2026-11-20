using System.ComponentModel.DataAnnotations;

namespace Acadex.Api.Models.Entities;

public class SchoolClass
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Section { get; set; }

    [MaxLength(250)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? AcademicYear { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
