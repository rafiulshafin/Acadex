using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AcadexDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Ensuring database schema exists...");
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already seeded. Skipping initial seeding.");
                return;
            }

            logger.LogInformation("Seeding initial data...");

            // 1. Classes
            var csClass = new SchoolClass
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Computer Science 2026",
                Section = "Section A",
                Description = "Senior undergraduate batch in Computer Science & Engineering",
                AcademicYear = "2026-2027",
                CreatedAt = DateTime.UtcNow
            };

            var seClass = new SchoolClass
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Software Engineering",
                Section = "Batch 26",
                Description = "Specialized stream for Distributed Systems and DevOps",
                AcademicYear = "2026-2027",
                CreatedAt = DateTime.UtcNow
            };

            await context.SchoolClasses.AddRangeAsync(csClass, seClass);

            // 2. Subjects
            var webEngineering = new Subject
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                Code = "SWE401",
                Name = "Full-Stack Web Engineering",
                Description = "Modern SPA architecture, RESTful API design, and asynchronous patterns",
                CreatedAt = DateTime.UtcNow
            };

            var databaseSystems = new Subject
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                Code = "CS302",
                Name = "Database Systems & Architecture",
                Description = "Relational data modeling, ACID transactions, and query optimization",
                CreatedAt = DateTime.UtcNow
            };

            var devopsCloud = new Subject
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "CS405",
                Name = "Cloud & DevOps Infrastructure",
                Description = "Containerization with Docker, CI/CD pipelines, and microservices scaling",
                CreatedAt = DateTime.UtcNow
            };

            await context.Subjects.AddRangeAsync(webEngineering, databaseSystems, devopsCloud);

            // 3. Demo Users
            // Passwords according to specification
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345");
            var teacherPasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@12345");
            var studentPasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@12345");

            var adminUser = new User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                FullName = "System Administrator",
                Email = "admin@acadex.edu",
                PasswordHash = adminPasswordHash,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var primaryTeacher = new User
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                FullName = "Dr. Robert Smith",
                Email = "teacher@acadex.edu",
                PasswordHash = teacherPasswordHash,
                Role = UserRole.Teacher,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var secondaryTeacher = new User
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc"),
                FullName = "Prof. Sarah Connor",
                Email = "sarah.teacher@acadex.edu",
                PasswordHash = teacherPasswordHash,
                Role = UserRole.Teacher,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var primaryStudent = new User
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                FullName = "John Doe",
                Email = "student@acadex.edu",
                PasswordHash = studentPasswordHash,
                Role = UserRole.Student,
                SchoolClassId = csClass.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var secondaryStudent = new User
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccd"),
                FullName = "Alex Rivera",
                Email = "alex.student@acadex.edu",
                PasswordHash = studentPasswordHash,
                Role = UserRole.Student,
                SchoolClassId = csClass.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(adminUser, primaryTeacher, secondaryTeacher, primaryStudent, secondaryStudent);

            // 4. Class Subject Teacher Allocations
            var alloc1 = new ClassSubject
            {
                Id = Guid.NewGuid(),
                SchoolClassId = csClass.Id,
                SubjectId = webEngineering.Id,
                TeacherId = primaryTeacher.Id,
                CreatedAt = DateTime.UtcNow
            };

            var alloc2 = new ClassSubject
            {
                Id = Guid.NewGuid(),
                SchoolClassId = csClass.Id,
                SubjectId = databaseSystems.Id,
                TeacherId = secondaryTeacher.Id,
                CreatedAt = DateTime.UtcNow
            };

            var alloc3 = new ClassSubject
            {
                Id = Guid.NewGuid(),
                SchoolClassId = seClass.Id,
                SubjectId = devopsCloud.Id,
                TeacherId = primaryTeacher.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.ClassSubjects.AddRangeAsync(alloc1, alloc2, alloc3);

            // 5. Sample Assignments
            var assignment1 = new Assignment
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Title = "Assignment 1: Distributed Assignment & Submission System Architecture",
                Description = "Design and implement a role-based Assignment & Submission Management system with ASP.NET Core Web API, Next.js, and PostgreSQL. Ensure JWT authentication, containerization with Docker, and CI/CD automation.",
                SchoolClassId = csClass.Id,
                SubjectId = webEngineering.Id,
                TeacherId = primaryTeacher.Id,
                Deadline = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100.00m,
                Status = AssignmentStatus.Published,
                AllowLateSubmissions = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var assignment2 = new Assignment
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Title = "Assignment 2: Entity Framework Core Relational Modeling",
                Description = "Construct normalized schemas with composite keys, navigation properties, and automated migrations. Implement audit fields and foreign key cascade constraints.",
                SchoolClassId = csClass.Id,
                SubjectId = databaseSystems.Id,
                TeacherId = secondaryTeacher.Id,
                Deadline = DateTime.UtcNow.AddDays(14),
                MaxMarks = 100.00m,
                Status = AssignmentStatus.Published,
                AllowLateSubmissions = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var assignment3Draft = new Assignment
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Title = "Draft Assignment: Next.js SSR and Streaming Optimizations",
                Description = "Investigate React Server Components, server actions, and optimistic UI rendering for high-performance dashboards.",
                SchoolClassId = csClass.Id,
                SubjectId = webEngineering.Id,
                TeacherId = primaryTeacher.Id,
                Deadline = DateTime.UtcNow.AddDays(21),
                MaxMarks = 50.00m,
                Status = AssignmentStatus.Draft,
                AllowLateSubmissions = false,
                CreatedAt = DateTime.UtcNow
            };

            await context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3Draft);

            // 6. Sample Submissions
            var submission1 = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment1.Id,
                StudentId = primaryStudent.Id,
                Content = "Completed the full-stack system using ASP.NET Core 8 Web API, Next.js App Router, Docker Compose, and PostgreSQL. Included JWT token authentication and role guards.",
                AttachmentUrl = "https://github.com/rafiulshafin/Acadex.git",
                Status = SubmissionStatus.Graded,
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                MarksAwarded = 95.00m,
                Feedback = "Excellent architecture! Clean separation of concerns, robust validation, and flawless Docker setup.",
                GradedAt = DateTime.UtcNow.AddHours(-6),
                GradedById = primaryTeacher.Id
            };

            var submission2 = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment2.Id,
                StudentId = primaryStudent.Id,
                Content = "Submitted initial relational schema diagram and EF Core migration script.",
                AttachmentUrl = "https://github.com/rafiulshafin/Acadex.git",
                Status = SubmissionStatus.UnderReview,
                SubmittedAt = DateTime.UtcNow.AddHours(-3)
            };

            await context.Submissions.AddRangeAsync(submission1, submission2);

            // 7. System Settings
            await context.SystemSettings.AddRangeAsync(
                new SystemSetting
                {
                    Key = "SystemName",
                    Value = "Acadex Academic Management System",
                    Description = "Application branding name",
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Key = "AllowLateSubmissionsDefault",
                    Value = "true",
                    Description = "Global default for late submission allowances",
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Key = "MaxFileSizeMB",
                    Value = "25",
                    Description = "Maximum attachment size in megabytes",
                    UpdatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeded successfully with Demo accounts (Admin, Teacher, Student).");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
