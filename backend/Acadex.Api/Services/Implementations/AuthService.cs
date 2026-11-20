using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Acadex.Api.Common.Exceptions;
using Acadex.Api.Data;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Acadex.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AcadexDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AcadexDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.SchoolClass)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = GenerateJwtToken(user, expiresAt);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                SchoolClassId = user.SchoolClassId,
                SchoolClassName = user.SchoolClass != null ? $"{user.SchoolClass.Name} {user.SchoolClass.Section}".Trim() : null
            }
        };
    }

    public async Task<UserProfileDto> GetCurrentUserProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.SchoolClass)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("User profile not found");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            SchoolClassId = user.SchoolClassId,
            SchoolClassName = user.SchoolClass != null ? $"{user.SchoolClass.Name} {user.SchoolClass.Section}".Trim() : null
        };
    }

    private string GenerateJwtToken(Models.Entities.User user, DateTime expiresAt)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? "Acadex_SuperSecret_Jwt_Security_Key_2026_Minimum32Characters!";
        var issuer = _configuration["Jwt:Issuer"] ?? "AcadexApi";
        var audience = _configuration["Jwt:Audience"] ?? "AcadexClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.SchoolClassId.HasValue)
        {
            claims.Add(new Claim("SchoolClassId", user.SchoolClassId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
