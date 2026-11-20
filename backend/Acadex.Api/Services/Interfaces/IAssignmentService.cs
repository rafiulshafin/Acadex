using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Enums;

namespace Acadex.Api.Services.Interfaces;

public interface IAssignmentService
{
    Task<List<AssignmentDto>> GetAssignmentsAsync(Guid currentUserId, UserRole role, Guid? classId = null, Guid? subjectId = null, AssignmentStatus? status = null);
    Task<AssignmentDto> GetAssignmentByIdAsync(Guid id, Guid currentUserId, UserRole role);
    Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentRequest request, Guid teacherId);
    Task<AssignmentDto> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request, Guid teacherId, bool isAdmin);
    Task<bool> DeleteAssignmentAsync(Guid id, Guid teacherId, bool isAdmin);
    Task<AssignmentDto> ChangeAssignmentStatusAsync(Guid id, AssignmentStatus status, Guid teacherId, bool isAdmin);
}
