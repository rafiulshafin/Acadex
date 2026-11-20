using Acadex.Api.Common.Exceptions;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Implementations;
using Acadex.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Acadex.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        var myConfiguration = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "A_Very_Long_And_Secure_Key_For_Testing_Purposes_Only_12345!"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUserProfile()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var password = "SecurePassword123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Teacher",
            Email = "teacher@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Teacher,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);

        // Act
        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "teacher@test.com",
            Password = password
        });

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be("teacher@test.com");
        result.User.Role.Should().Be(UserRole.Teacher);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test Student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"),
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);

        // Act & Assert
        var act = async () => await service.LoginAsync(new LoginRequest
        {
            Email = "student@test.com",
            Password = "WrongPassword!"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Deactivated User",
            Email = "inactive@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Student,
            IsActive = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);

        // Act & Assert
        var act = async () => await service.LoginAsync(new LoginRequest
        {
            Email = "inactive@test.com",
            Password = "Password123!"
        });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
