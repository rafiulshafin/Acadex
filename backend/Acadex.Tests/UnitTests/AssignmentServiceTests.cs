using Acadex.Api.Common.Exceptions;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Implementations;
using Acadex.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace Acadex.Tests.UnitTests;

public class AssignmentServiceTests
{
    [Fact]
    public async Task CreateAssignment_WhenTeacherIsAllocated_CreatesSuccessfully()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Prof. Turing", Email = "turing@test.com", Role = UserRole.Teacher };
        var schoolClass = new SchoolClass { Id = Guid.NewGuid(), Name = "CS 101" };
        var subject = new Subject { Id = Guid.NewGuid(), Code = "CS101", Name = "Intro to CS" };

        var allocation = new ClassSubject
        {
            Id = Guid.NewGuid(),
            SchoolClassId = schoolClass.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id
        };

        context.Users.Add(teacher);
        context.SchoolClasses.Add(schoolClass);
        context.Subjects.Add(subject);
        context.ClassSubjects.Add(allocation);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);

        var request = new CreateAssignmentRequest
        {
            Title = "Recursion Homework",
            Description = "Solve Hanoi towers",
            SchoolClassId = schoolClass.Id,
            SubjectId = subject.Id,
            Deadline = DateTime.UtcNow.AddDays(5),
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };

        // Act
        var result = await service.CreateAssignmentAsync(request, teacher.Id);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Recursion Homework");
        result.TeacherId.Should().Be(teacher.Id);
        result.Status.Should().Be(AssignmentStatus.Published);
    }

    [Fact]
    public async Task CreateAssignment_WhenTeacherIsNotAllocated_ThrowsForbiddenException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Prof. Turing", Email = "turing@test.com", Role = UserRole.Teacher };
        var schoolClass = new SchoolClass { Id = Guid.NewGuid(), Name = "Bio 101" };
        var subject = new Subject { Id = Guid.NewGuid(), Code = "BIO101", Name = "Intro to Bio" };

        context.Users.Add(teacher);
        context.SchoolClasses.Add(schoolClass);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);

        var request = new CreateAssignmentRequest
        {
            Title = "Genetics Lab",
            Description = "DNA extraction",
            SchoolClassId = schoolClass.Id,
            SubjectId = subject.Id,
            Deadline = DateTime.UtcNow.AddDays(5),
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };

        // Act & Assert
        var act = async () => await service.CreateAssignmentAsync(request, teacher.Id);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*not assigned to teach this subject*");
    }

    [Fact]
    public async Task GetAssignments_StudentOnlySeesPublishedAssignmentsForTheirClass()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var myClass = new SchoolClass { Id = Guid.NewGuid(), Name = "Class A" };
        var otherClass = new SchoolClass { Id = Guid.NewGuid(), Name = "Class B" };
        var subject = new Subject { Id = Guid.NewGuid(), Code = "CS101", Name = "Intro" };
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", Role = UserRole.Teacher };

        var student = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Student Alice",
            Email = "alice@test.com",
            Role = UserRole.Student,
            SchoolClassId = myClass.Id
        };

        // 1. Published for student's class (should see)
        var a1 = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "Class A Homework",
            Description = "Solve problem 1",
            SchoolClassId = myClass.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            Status = AssignmentStatus.Published,
            Deadline = DateTime.UtcNow.AddDays(3)
        };

        // 2. Draft for student's class (should NOT see)
        var a2Draft = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "Draft Assignment",
            Description = "Not yet ready",
            SchoolClassId = myClass.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            Status = AssignmentStatus.Draft,
            Deadline = DateTime.UtcNow.AddDays(3)
        };

        // 3. Published for other class (should NOT see)
        var a3OtherClass = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "Class B Homework",
            Description = "Different class",
            SchoolClassId = otherClass.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            Status = AssignmentStatus.Published,
            Deadline = DateTime.UtcNow.AddDays(3)
        };

        context.Users.AddRange(teacher, student);
        context.SchoolClasses.AddRange(myClass, otherClass);
        context.Subjects.Add(subject);
        context.Assignments.AddRange(a1, a2Draft, a3OtherClass);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);

        // Act
        var result = await service.GetAssignmentsAsync(student.Id, UserRole.Student);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Class A Homework");
    }
}
