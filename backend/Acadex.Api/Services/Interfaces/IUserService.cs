using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUsersAsync(UserRole? role = null, string? search = null);
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
}
