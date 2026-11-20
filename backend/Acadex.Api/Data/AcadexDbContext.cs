using Acadex.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Data;

public class AcadexDbContext : DbContext
{
    public AcadexDbContext(DbContextOptions<AcadexDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSubject> ClassSubjects => Set<ClassSubject>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User indexes & relationships
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.SchoolClass)
                .WithMany(c => c.Students)
                .HasForeignKey(u => u.SchoolClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Subject index
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasIndex(s => s.Code).IsUnique();
        });

        // ClassSubject composite unique index & relationships
        modelBuilder.Entity<ClassSubject>(entity =>
        {
            entity.HasIndex(cs => new { cs.SchoolClassId, cs.SubjectId }).IsUnique();

            entity.HasOne(cs => cs.SchoolClass)
                .WithMany(c => c.ClassSubjects)
                .HasForeignKey(cs => cs.SchoolClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cs => cs.Subject)
                .WithMany(s => s.ClassSubjects)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cs => cs.Teacher)
                .WithMany(u => u.TeacherAllocations)
                .HasForeignKey(cs => cs.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Assignment relationships
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasOne(a => a.SchoolClass)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Subject)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Teacher)
                .WithMany(u => u.CreatedAssignments)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Submission relationships and composite unique (Student can have one submission per assignment, updated before deadline)
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();

            entity.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.GradedBy)
                .WithMany()
                .HasForeignKey(s => s.GradedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
