using Acadex.Api.Common.Exceptions;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Implementations;
using Acadex.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace Acadex.Tests.UnitTests;

public class SubmissionServiceTests
{
    [Fact]
    public async Task SubmitAssignment_BeforeDeadline_CreatesSubmissionSuccessfully()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var schoolClass = new SchoolClass { Id = Guid.NewGuid(), Name = "Class A" };
        var subject = new Subject { Id = Guid.NewGuid(), Code = "SWE", Name = "Software Eng" };
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", Role = UserRole.Teacher };
        var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", Role = UserRole.Student, SchoolClassId = schoolClass.Id };

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "DevOps Project",
            Description = "Docker compose",
            SchoolClassId = schoolClass.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            Status = AssignmentStatus.Published,
            Deadline = DateTime.UtcNow.AddDays(2),
            AllowLateSubmissions = false,
            MaxMarks = 100
        };

        context.Users.AddRange(teacher, student);
        context.SchoolClasses.Add(schoolClass);
        context.Subjects.Add(subject);
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var service = new SubmissionService(context);

        var request = new CreateSubmissionRequest
        {
            AssignmentId = assignment.Id,
            Content = "Here is my project repo: https://github.com/test/repo",
            AttachmentUrl = "https://github.com/test/repo"
        };

        // Act
        var result = await service.SubmitAssignmentAsync(request, student.Id);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(student.Id);
        result.Status.Should().Be(SubmissionStatus.Submitted);
        result.IsLate.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitAssignment_AfterDeadline_WhenLateNotAllowed_ThrowsAppException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var schoolClass = new SchoolClass { Id = Guid.NewGuid(), Name = "Class A" };
        var subject = new Subject { Id = Guid.NewGuid(), Code = "SWE", Name = "Software Eng" };
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", Role = UserRole.Teacher };
        var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", Role = UserRole.Student, SchoolClassId = schoolClass.Id };

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "Past Due Assignment",
            Description = "Strict deadline",
            SchoolClassId = schoolClass.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            Status = AssignmentStatus.Published,
            Deadline = DateTime.UtcNow.AddDays(-1), // Past deadline!
            AllowLateSubmissions = false,
            MaxMarks = 100
        };

        context.Users.AddRange(teacher, student);
        context.SchoolClasses.Add(schoolClass);
        context.Subjects.Add(subject);
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var service = new SubmissionService(context);

        var request = new CreateSubmissionRequest
        {
            AssignmentId = assignment.Id,
            Content = "Late submission attempt"
        };

        // Act & Assert
        var act = async () => await service.SubmitAssignmentAsync(request, student.Id);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("*deadline has passed*");
    }

    [Fact]
    public async Task GradeSubmission_WithValidMarks_UpdatesMarksAndFeedback()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", Role = UserRole.Teacher };
        var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", Role = UserRole.Student };

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "Math Exam",
            Description = "Calculus",
            TeacherId = teacher.Id,
            MaxMarks = 100,
            Deadline = DateTime.UtcNow.AddDays(5)
        };

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignment.Id,
            Assignment = assignment,
            StudentId = student.Id,
            Student = student,
            Content = "My calculus answers",
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.Users.AddRange(teacher, student);
        context.Assignments.Add(assignment);
        context.Submissions.Add(submission);
        await context.SaveChangesAsync();

        var service = new SubmissionService(context);

        var gradeRequest = new GradeSubmissionRequest
        {
            MarksAwarded = 92.5m,
            Feedback = "Well structured proofs!",
            Status = SubmissionStatus.Graded
        };

        // Act
        var result = await service.GradeSubmissionAsync(submission.Id, gradeRequest, teacher.Id, false);

        // Assert
        result.MarksAwarded.Should().Be(92.5m);
        result.Feedback.Should().Be("Well structured proofs!");
        result.Status.Should().Be(SubmissionStatus.Graded);
    }

    [Fact]
    public async Task GradeSubmission_WithMarksExceedingMax_ThrowsAppException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryContext();
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", Role = UserRole.Teacher };
        var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", Role = UserRole.Student };

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = "Quiz 1",
            Description = "Quick quiz",
            TeacherId = teacher.Id,
            MaxMarks = 20, // Max marks is 20!
            Deadline = DateTime.UtcNow.AddDays(5)
        };

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignment.Id,
            Assignment = assignment,
            StudentId = student.Id,
            Student = student,
            Content = "My answers",
            Status = SubmissionStatus.Submitted
        };

        context.Users.AddRange(teacher, student);
        context.Assignments.Add(assignment);
        context.Submissions.Add(submission);
        await context.SaveChangesAsync();

        var service = new SubmissionService(context);

        var gradeRequest = new GradeSubmissionRequest
        {
            MarksAwarded = 25m, // Exceeds 20!
            Feedback = "Invalid marks"
        };

        // Act & Assert
        var act = async () => await service.GradeSubmissionAsync(submission.Id, gradeRequest, teacher.Id, false);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("*between 0 and 20*");
    }
}
