using Acadex.Api.Models.DTOs;

namespace Acadex.Api.Services.Interfaces;

public interface IClassSubjectService
{
    // Classes
    Task<List<SchoolClassDto>> GetClassesAsync();
    Task<SchoolClassDto> GetClassByIdAsync(Guid id);
    Task<SchoolClassDto> CreateClassAsync(CreateSchoolClassRequest request);
    Task<bool> DeleteClassAsync(Guid id);

    // Subjects
    Task<List<SubjectDto>> GetSubjectsAsync();
    Task<SubjectDto> GetSubjectByIdAsync(Guid id);
    Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequest request);
    Task<bool> DeleteSubjectAsync(Guid id);

    // Allocations (Teacher -> Class + Subject)
    Task<List<ClassSubjectDto>> GetAllocationsAsync(Guid? teacherId = null, Guid? classId = null);
    Task<ClassSubjectDto> AssignTeacherAsync(AssignTeacherRequest request);
    Task<bool> RemoveAllocationAsync(Guid id);
}
