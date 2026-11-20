using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Services.Interfaces;

public interface ISubmissionService
{
    Task<List<SubmissionDto>> GetSubmissionsAsync(Guid currentUserId, UserRole role, Guid? assignmentId = null, SubmissionStatus? status = null);
    Task<SubmissionDto> GetSubmissionByIdAsync(Guid id, Guid currentUserId, UserRole role);
    Task<SubmissionDto> SubmitAssignmentAsync(CreateSubmissionRequest request, Guid studentId);
    Task<SubmissionDto> UpdateSubmissionAsync(Guid submissionId, UpdateSubmissionRequest request, Guid studentId);
    Task<SubmissionDto> GradeSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid teacherId, bool isAdmin);
    Task<SubmissionDto> UpdateSubmissionStatusAsync(Guid submissionId, SubmissionStatus status, Guid teacherId, bool isAdmin);
}
