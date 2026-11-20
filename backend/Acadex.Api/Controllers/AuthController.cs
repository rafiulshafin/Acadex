using Acadex.Api.Common;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadex.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService _authService)
    {
        this._authService = _authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetMe()
    {
        var user = await _authService.GetCurrentUserProfileAsync(CurrentUserId);
        return Ok(ApiResponse<UserProfileDto>.Ok(user, "User profile retrieved successfully"));
    }
}
