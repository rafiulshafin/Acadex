using System.ComponentModel.DataAnnotations;

namespace Acadex.Api.Models.Entities;

public class ClassSubject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SchoolClassId { get; set; }
    public SchoolClass SchoolClass { get; set; } = null!;

    [Required]
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    [Required]
    public Guid TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
