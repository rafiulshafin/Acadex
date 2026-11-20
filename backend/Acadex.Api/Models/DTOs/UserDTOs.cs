using System.ComponentModel.DataAnnotations;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Models.DTOs;

public class CreateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public Guid? SchoolClassId { get; set; }
}

public class UpdateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public UserRole Role { get; set; }

    public Guid? SchoolClassId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string RoleName => Role.ToString();
    public Guid? SchoolClassId { get; set; }
    public string? SchoolClassName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
