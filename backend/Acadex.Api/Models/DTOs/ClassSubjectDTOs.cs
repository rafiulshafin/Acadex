using System.ComponentModel.DataAnnotations;

namespace Acadex.Api.Models.DTOs;

public class CreateSchoolClassRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Section { get; set; }

    [MaxLength(250)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? AcademicYear { get; set; }
}

public class SchoolClassDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Description { get; set; }
    public string? AcademicYear { get; set; }
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSubjectRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }
}

public class SubjectDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignTeacherRequest
{
    [Required]
    public Guid SchoolClassId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    [Required]
    public Guid TeacherId { get; set; }
}

public class ClassSubjectDto
{
    public Guid Id { get; set; }
    public Guid SchoolClassId { get; set; }
    public string SchoolClassName { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
