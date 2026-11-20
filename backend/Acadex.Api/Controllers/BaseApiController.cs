using System.Security.Claims;
using Acadex.Api.Common.Exceptions;
using Acadex.Api.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            {
                throw new UnauthorizedException("User identity not found in token");
            }
            return userId;
        }
    }

    protected UserRole CurrentUserRole
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(claim) || !Enum.TryParse<UserRole>(claim, true, out var role))
            {
                throw new UnauthorizedException("User role not recognized");
            }
            return role;
        }
    }

    protected bool IsAdmin => CurrentUserRole == UserRole.Admin;
    protected bool IsTeacher => CurrentUserRole == UserRole.Teacher;
    protected bool IsStudent => CurrentUserRole == UserRole.Student;
}
