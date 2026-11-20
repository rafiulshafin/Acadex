using Acadex.Api.Common.Exceptions;
using Acadex.Api.Data;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Services.Implementations;

public class SubmissionService : ISubmissionService
{
    private readonly AcadexDbContext _context;

    public SubmissionService(AcadexDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubmissionDto>> GetSubmissionsAsync(
        Guid currentUserId,
        UserRole role,
        Guid? assignmentId = null,
        SubmissionStatus? status = null)
    {
        var query = _context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .Include(s => s.GradedBy)
            .AsNoTracking()
            .AsQueryable();

        if (role == UserRole.Student)
        {
            // Student only sees their own submissions
            query = query.Where(s => s.StudentId == currentUserId);
        }
        else if (role == UserRole.Teacher)
        {
            // Teacher sees submissions for assignments they teach or created
            query = query.Where(s => s.Assignment.TeacherId == currentUserId);
        }

        if (assignmentId.HasValue)
        {
            query = query.Where(s => s.AssignmentId == assignmentId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        return await query
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new SubmissionDto
            {
                Id = s.Id,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                AssignmentMaxMarks = s.Assignment.MaxMarks,
                AssignmentDeadline = s.Assignment.Deadline,
                StudentId = s.StudentId,
                StudentName = s.Student.FullName,
                StudentEmail = s.Student.Email,
                Content = s.Content,
                AttachmentUrl = s.AttachmentUrl,
                Status = s.Status,
                SubmittedAt = s.SubmittedAt,
                UpdatedAt = s.UpdatedAt,
                IsLate = s.SubmittedAt > s.Assignment.Deadline,
                MarksAwarded = s.MarksAwarded,
                Feedback = s.Feedback,
                GradedAt = s.GradedAt,
                GradedByName = s.GradedBy != null ? s.GradedBy.FullName : null
            })
            .ToListAsync();
    }

    public async Task<SubmissionDto> GetSubmissionByIdAsync(Guid id, Guid currentUserId, UserRole role)
    {
        var s = await _context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .Include(s => s.GradedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (s == null)
        {
            throw new NotFoundException($"Submission with ID {id} not found");
        }

        if (role == UserRole.Student && s.StudentId != currentUserId)
        {
            throw new ForbiddenException("You do not have permission to view this submission");
        }

        if (role == UserRole.Teacher && s.Assignment.TeacherId != currentUserId)
        {
            throw new ForbiddenException("You can only view submissions for your assigned assignments");
        }

        return new SubmissionDto
        {
            Id = s.Id,
            AssignmentId = s.AssignmentId,
            AssignmentTitle = s.Assignment.Title,
            AssignmentMaxMarks = s.Assignment.MaxMarks,
            AssignmentDeadline = s.Assignment.Deadline,
            StudentId = s.StudentId,
            StudentName = s.Student.FullName,
            StudentEmail = s.Student.Email,
            Content = s.Content,
            AttachmentUrl = s.AttachmentUrl,
            Status = s.Status,
            SubmittedAt = s.SubmittedAt,
            UpdatedAt = s.UpdatedAt,
            IsLate = s.SubmittedAt > s.Assignment.Deadline,
            MarksAwarded = s.MarksAwarded,
            Feedback = s.Feedback,
            GradedAt = s.GradedAt,
            GradedByName = s.GradedBy != null ? s.GradedBy.FullName : null
        };
    }

    public async Task<SubmissionDto> SubmitAssignmentAsync(CreateSubmissionRequest request, Guid studentId)
    {
        var student = await _context.Users.FindAsync(studentId);
        if (student == null || student.Role != UserRole.Student)
        {
            throw new ForbiddenException("Only students can submit assignments");
        }

        var assignment = await _context.Assignments.FindAsync(request.AssignmentId);
        if (assignment == null)
        {
            throw new NotFoundException("Assignment not found");
        }

        if (assignment.Status != AssignmentStatus.Published)
        {
            throw new AppException("Cannot submit to an assignment that is not published");
        }

        if (student.SchoolClassId != assignment.SchoolClassId)
        {
            throw new ForbiddenException("You are not enrolled in the class for this assignment");
        }

        var now = DateTime.UtcNow;
        if (now > assignment.Deadline && !assignment.AllowLateSubmissions)
        {
            throw new AppException("Assignment deadline has passed and late submissions are not allowed");
        }

        var existing = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == studentId);

        if (existing != null)
        {
            throw new ConflictException("You have already submitted for this assignment. Please use the update option.");
        }

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = request.AssignmentId,
            StudentId = studentId,
            Content = request.Content.Trim(),
            AttachmentUrl = request.AttachmentUrl?.Trim(),
            Status = SubmissionStatus.Submitted,
            SubmittedAt = now
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return await GetSubmissionByIdAsync(submission.Id, studentId, UserRole.Student);
    }

    public async Task<SubmissionDto> UpdateSubmissionAsync(Guid submissionId, UpdateSubmissionRequest request, Guid studentId)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Submission not found");
        }

        if (submission.StudentId != studentId)
        {
            throw new ForbiddenException("You can only modify your own submission");
        }

        if (submission.Status == SubmissionStatus.Graded)
        {
            throw new AppException("Cannot modify a submission that has already been graded");
        }

        var now = DateTime.UtcNow;
        // Allowed if before deadline, or if teacher specifically permitted resubmission
        if (now > submission.Assignment.Deadline && submission.Status != SubmissionStatus.ResubmissionAllowed)
        {
            throw new AppException("Submission cannot be modified after deadline unless teacher granted resubmission");
        }

        submission.Content = request.Content.Trim();
        submission.AttachmentUrl = request.AttachmentUrl?.Trim();
        submission.UpdatedAt = now;
        submission.Status = SubmissionStatus.Submitted;

        await _context.SaveChangesAsync();
        return await GetSubmissionByIdAsync(submission.Id, studentId, UserRole.Student);
    }

    public async Task<SubmissionDto> GradeSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid teacherId, bool isAdmin)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Submission not found");
        }

        if (!isAdmin && submission.Assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You can only grade submissions for your assigned assignments");
        }

        if (request.MarksAwarded < 0 || request.MarksAwarded > submission.Assignment.MaxMarks)
        {
            throw new AppException($"Marks must be between 0 and {submission.Assignment.MaxMarks}");
        }

        submission.MarksAwarded = request.MarksAwarded;
        submission.Feedback = request.Feedback?.Trim();
        submission.Status = request.Status;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradedById = teacherId;

        await _context.SaveChangesAsync();
        return await GetSubmissionByIdAsync(submission.Id, teacherId, isAdmin ? UserRole.Admin : UserRole.Teacher);
    }

    public async Task<SubmissionDto> UpdateSubmissionStatusAsync(Guid submissionId, SubmissionStatus status, Guid teacherId, bool isAdmin)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Submission not found");
        }

        if (!isAdmin && submission.Assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You can only update status for your assigned assignments");
        }

        submission.Status = status;
        await _context.SaveChangesAsync();

        return await GetSubmissionByIdAsync(submission.Id, teacherId, isAdmin ? UserRole.Admin : UserRole.Teacher);
    }
}
