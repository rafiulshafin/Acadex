using Acadex.Api.Common.Exceptions;
using Acadex.Api.Data;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Services.Implementations;

public class AssignmentService : IAssignmentService
{
    private readonly AcadexDbContext _context;

    public AssignmentService(AcadexDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignmentDto>> GetAssignmentsAsync(
        Guid currentUserId,
        UserRole role,
        Guid? classId = null,
        Guid? subjectId = null,
        AssignmentStatus? status = null)
    {
        var query = _context.Assignments
            .Include(a => a.SchoolClass)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .AsNoTracking()
            .AsQueryable();

        if (role == UserRole.Student)
        {
            var student = await _context.Users.FindAsync(currentUserId);
            if (student == null || !student.SchoolClassId.HasValue)
            {
                return new List<AssignmentDto>();
            }

            // Students only see published assignments for their enrolled class
            query = query.Where(a => a.SchoolClassId == student.SchoolClassId.Value && a.Status == AssignmentStatus.Published);
        }
        else if (role == UserRole.Teacher)
        {
            // Teacher sees assignments they created or allocated to them
            var allocatedClassSubjectIds = await _context.ClassSubjects
                .Where(cs => cs.TeacherId == currentUserId)
                .Select(cs => new { cs.SchoolClassId, cs.SubjectId })
                .ToListAsync();

            query = query.Where(a => a.TeacherId == currentUserId ||
                allocatedClassSubjectIds.Any(alloc => alloc.SchoolClassId == a.SchoolClassId && alloc.SubjectId == a.SubjectId));
        }

        if (classId.HasValue)
        {
            query = query.Where(a => a.SchoolClassId == classId.Value);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(a => a.SubjectId == subjectId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var assignments = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return assignments.Select(a =>
        {
            var mySub = role == UserRole.Student
                ? a.Submissions.FirstOrDefault(s => s.StudentId == currentUserId)
                : null;

            return new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                SchoolClassId = a.SchoolClassId,
                SchoolClassName = $"{a.SchoolClass.Name} {a.SchoolClass.Section}".Trim(),
                SubjectId = a.SubjectId,
                SubjectCode = a.Subject.Code,
                SubjectName = a.Subject.Name,
                TeacherId = a.TeacherId,
                TeacherName = a.Teacher.FullName,
                Deadline = a.Deadline,
                MaxMarks = a.MaxMarks,
                Status = a.Status,
                AllowLateSubmissions = a.AllowLateSubmissions,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                TotalSubmissions = a.Submissions.Count,
                GradedSubmissions = a.Submissions.Count(s => s.Status == SubmissionStatus.Graded),
                HasSubmitted = mySub != null,
                MySubmission = mySub != null ? new SubmissionDto
                {
                    Id = mySub.Id,
                    AssignmentId = mySub.AssignmentId,
                    AssignmentTitle = a.Title,
                    AssignmentMaxMarks = a.MaxMarks,
                    AssignmentDeadline = a.Deadline,
                    StudentId = mySub.StudentId,
                    Content = mySub.Content,
                    AttachmentUrl = mySub.AttachmentUrl,
                    Status = mySub.Status,
                    SubmittedAt = mySub.SubmittedAt,
                    UpdatedAt = mySub.UpdatedAt,
                    IsLate = mySub.SubmittedAt > a.Deadline,
                    MarksAwarded = mySub.MarksAwarded,
                    Feedback = mySub.Feedback,
                    GradedAt = mySub.GradedAt
                } : null
            };
        }).ToList();
    }

    public async Task<AssignmentDto> GetAssignmentByIdAsync(Guid id, Guid currentUserId, UserRole role)
    {
        var a = await _context.Assignments
            .Include(a => a.SchoolClass)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (a == null)
        {
            throw new NotFoundException($"Assignment with ID {id} not found");
        }

        if (role == UserRole.Student)
        {
            var student = await _context.Users.FindAsync(currentUserId);
            if (student == null || student.SchoolClassId != a.SchoolClassId || a.Status != AssignmentStatus.Published)
            {
                throw new ForbiddenException("You do not have permission to view this assignment");
            }
        }

        var mySub = role == UserRole.Student
            ? a.Submissions.FirstOrDefault(s => s.StudentId == currentUserId)
            : null;

        return new AssignmentDto
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            SchoolClassId = a.SchoolClassId,
            SchoolClassName = $"{a.SchoolClass.Name} {a.SchoolClass.Section}".Trim(),
            SubjectId = a.SubjectId,
            SubjectCode = a.Subject.Code,
            SubjectName = a.Subject.Name,
            TeacherId = a.TeacherId,
            TeacherName = a.Teacher.FullName,
            Deadline = a.Deadline,
            MaxMarks = a.MaxMarks,
            Status = a.Status,
            AllowLateSubmissions = a.AllowLateSubmissions,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            TotalSubmissions = a.Submissions.Count,
            GradedSubmissions = a.Submissions.Count(s => s.Status == SubmissionStatus.Graded),
            HasSubmitted = mySub != null,
            MySubmission = mySub != null ? new SubmissionDto
            {
                Id = mySub.Id,
                AssignmentId = mySub.AssignmentId,
                AssignmentTitle = a.Title,
                AssignmentMaxMarks = a.MaxMarks,
                AssignmentDeadline = a.Deadline,
                StudentId = mySub.StudentId,
                Content = mySub.Content,
                AttachmentUrl = mySub.AttachmentUrl,
                Status = mySub.Status,
                SubmittedAt = mySub.SubmittedAt,
                UpdatedAt = mySub.UpdatedAt,
                IsLate = mySub.SubmittedAt > a.Deadline,
                MarksAwarded = mySub.MarksAwarded,
                Feedback = mySub.Feedback,
                GradedAt = mySub.GradedAt
            } : null
        };
    }

    public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentRequest request, Guid teacherId)
    {
        // Verify class & subject exist
        var schoolClass = await _context.SchoolClasses.FindAsync(request.SchoolClassId);
        if (schoolClass == null) throw new NotFoundException("School Class not found");

        var subject = await _context.Subjects.FindAsync(request.SubjectId);
        if (subject == null) throw new NotFoundException("Subject not found");

        // Verify assignment allocation to teacher
        var isAllocated = await _context.ClassSubjects
            .AnyAsync(cs => cs.SchoolClassId == request.SchoolClassId && cs.SubjectId == request.SubjectId && cs.TeacherId == teacherId);

        if (!isAllocated)
        {
            // Also allow if user is admin (checked in controller) or allocated
            var user = await _context.Users.FindAsync(teacherId);
            if (user == null || (user.Role != UserRole.Admin && !isAllocated))
            {
                throw new ForbiddenException("You are not assigned to teach this subject for the selected class");
            }
        }

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            SchoolClassId = request.SchoolClassId,
            SubjectId = request.SubjectId,
            TeacherId = teacherId,
            Deadline = request.Deadline.ToUniversalTime(),
            MaxMarks = request.MaxMarks,
            Status = request.Status,
            AllowLateSubmissions = request.AllowLateSubmissions,
            CreatedAt = DateTime.UtcNow
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return await GetAssignmentByIdAsync(assignment.Id, teacherId, UserRole.Teacher);
    }

    public async Task<AssignmentDto> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request, Guid teacherId, bool isAdmin)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) throw new NotFoundException($"Assignment with ID {id} not found");

        if (!isAdmin && assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You can only modify assignments created by you");
        }

        assignment.Title = request.Title.Trim();
        assignment.Description = request.Description.Trim();
        assignment.Deadline = request.Deadline.ToUniversalTime();
        assignment.MaxMarks = request.MaxMarks;
        assignment.Status = request.Status;
        assignment.AllowLateSubmissions = request.AllowLateSubmissions;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetAssignmentByIdAsync(assignment.Id, teacherId, isAdmin ? UserRole.Admin : UserRole.Teacher);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id, Guid teacherId, bool isAdmin)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) throw new NotFoundException($"Assignment with ID {id} not found");

        if (!isAdmin && assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You can only delete assignments created by you");
        }

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AssignmentDto> ChangeAssignmentStatusAsync(Guid id, AssignmentStatus status, Guid teacherId, bool isAdmin)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) throw new NotFoundException($"Assignment with ID {id} not found");

        if (!isAdmin && assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You can only change status of assignments created by you");
        }

        assignment.Status = status;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetAssignmentByIdAsync(assignment.Id, teacherId, isAdmin ? UserRole.Admin : UserRole.Teacher);
    }
}
