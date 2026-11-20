using Acadex.Api.Common.Exceptions;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Implementations;
using Acadex.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace Acadex.Tests.UnitTests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_WithUniqueEmail_CreatesUserSuccessfully()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var service = new UserService(context);

        var request = new CreateUserRequest
        {
            FullName = "New Student",
            Email = "new.student@acadex.edu",
            Password = "Password123!",
            Role = UserRole.Student
        };

        // Act
        var result = await service.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("new.student@acadex.edu");
        result.Role.Should().Be(UserRole.Student);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Existing Teacher",
            Email = "teacher@acadex.edu",
            PasswordHash = "hash",
            Role = UserRole.Teacher
        });
        await context.SaveChangesAsync();

        var service = new UserService(context);

        var request = new CreateUserRequest
        {
            FullName = "Duplicate User",
            Email = "teacher@acadex.edu",
            Password = "Password123!",
            Role = UserRole.Teacher
        };

        // Act & Assert
        var act = async () => await service.CreateUserAsync(request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task DeleteUser_DeactivatesUser()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "User To Remove",
            Email = "remove@acadex.edu",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.DeleteUserAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Users.FindAsync(user.Id);
        updated!.IsActive.Should().BeFalse();
    }
}
