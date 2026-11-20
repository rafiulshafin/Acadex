using Acadex.Api.Models.DTOs;

namespace Acadex.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserProfileDto> GetCurrentUserProfileAsync(Guid userId);
}
