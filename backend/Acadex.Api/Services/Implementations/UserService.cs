using Acadex.Api.Common.Exceptions;
using Acadex.Api.Data;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Services.Implementations;

public class UserService : IUserService
{
    private readonly AcadexDbContext _context;

    public UserService(AcadexDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetUsersAsync(UserRole? role = null, string? search = null)
    {
        var query = _context.Users
            .Include(u => u.SchoolClass)
            .AsNoTracking()
            .AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(searchLower) || u.Email.ToLower().Contains(searchLower));
        }

        return await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                SchoolClassId = u.SchoolClassId,
                SchoolClassName = u.SchoolClass != null ? $"{u.SchoolClass.Name} {u.SchoolClass.Section}".Trim() : null,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var u = await _context.Users
            .Include(u => u.SchoolClass)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (u == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        return new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            SchoolClassId = u.SchoolClassId,
            SchoolClassName = u.SchoolClass != null ? $"{u.SchoolClass.Name} {u.SchoolClass.Section}".Trim() : null,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var emailLower = request.Email.Trim().ToLower();
        var existing = await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower);
        if (existing)
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists");
        }

        if (request.Role == UserRole.Student && request.SchoolClassId.HasValue)
        {
            var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == request.SchoolClassId.Value);
            if (!classExists)
            {
                throw new NotFoundException("Specified School Class does not exist");
            }
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = emailLower,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            SchoolClassId = request.Role == UserRole.Student ? request.SchoolClassId : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        var emailLower = request.Email.Trim().ToLower();
        var emailConflict = await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower && u.Id != id);
        if (emailConflict)
        {
            throw new ConflictException($"Email '{request.Email}' is already in use by another account");
        }

        user.FullName = request.FullName.Trim();
        user.Email = emailLower;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (request.Role == UserRole.Student)
        {
            user.SchoolClassId = request.SchoolClassId;
        }
        else
        {
            user.SchoolClassId = null;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _context.SaveChangesAsync();
        return await GetUserByIdAsync(user.Id);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        // Soft delete / deactivate to maintain referential integrity
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
