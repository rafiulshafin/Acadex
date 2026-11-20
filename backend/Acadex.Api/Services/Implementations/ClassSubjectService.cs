using Acadex.Api.Common.Exceptions;
using Acadex.Api.Data;
using Acadex.Api.Models.DTOs;
using Acadex.Api.Models.Entities;
using Acadex.Api.Models.Enums;
using Acadex.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Api.Services.Implementations;

public class ClassSubjectService : IClassSubjectService
{
    private readonly AcadexDbContext _context;

    public ClassSubjectService(AcadexDbContext context)
    {
        _context = context;
    }

    public async Task<List<SchoolClassDto>> GetClassesAsync()
    {
        return await _context.SchoolClasses
            .Include(c => c.Students)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Section)
            .Select(c => new SchoolClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section,
                Description = c.Description,
                AcademicYear = c.AcademicYear,
                StudentCount = c.Students.Count(s => s.IsActive),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SchoolClassDto> GetClassByIdAsync(Guid id)
    {
        var c = await _context.SchoolClasses
            .Include(c => c.Students)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (c == null)
        {
            throw new NotFoundException($"Class with ID {id} not found");
        }

        return new SchoolClassDto
        {
            Id = c.Id,
            Name = c.Name,
            Section = c.Section,
            Description = c.Description,
            AcademicYear = c.AcademicYear,
            StudentCount = c.Students.Count(s => s.IsActive),
            CreatedAt = c.CreatedAt
        };
    }

    public async Task<SchoolClassDto> CreateClassAsync(CreateSchoolClassRequest request)
    {
        var schoolClass = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Section = request.Section?.Trim(),
            Description = request.Description?.Trim(),
            AcademicYear = request.AcademicYear?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.SchoolClasses.Add(schoolClass);
        await _context.SaveChangesAsync();

        return await GetClassByIdAsync(schoolClass.Id);
    }

    public async Task<bool> DeleteClassAsync(Guid id)
    {
        var schoolClass = await _context.SchoolClasses.FirstOrDefaultAsync(c => c.Id == id);
        if (schoolClass == null)
        {
            throw new NotFoundException($"Class with ID {id} not found");
        }

        _context.SchoolClasses.Remove(schoolClass);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SubjectDto>> GetSubjectsAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SubjectDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SubjectDto> GetSubjectByIdAsync(Guid id)
    {
        var s = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (s == null)
        {
            throw new NotFoundException($"Subject with ID {id} not found");
        }

        return new SubjectDto
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            Description = s.Description,
            CreatedAt = s.CreatedAt
        };
    }

    public async Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequest request)
    {
        var codeUpper = request.Code.Trim().ToUpper();
        var exists = await _context.Subjects.AnyAsync(s => s.Code.ToUpper() == codeUpper);
        if (exists)
        {
            throw new ConflictException($"Subject code '{request.Code}' already exists");
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Code = codeUpper,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return await GetSubjectByIdAsync(subject.Id);
    }

    public async Task<bool> DeleteSubjectAsync(Guid id)
    {
        var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id);
        if (subject == null)
        {
            throw new NotFoundException($"Subject with ID {id} not found");
        }

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ClassSubjectDto>> GetAllocationsAsync(Guid? teacherId = null, Guid? classId = null)
    {
        var query = _context.ClassSubjects
            .Include(cs => cs.SchoolClass)
            .Include(cs => cs.Subject)
            .Include(cs => cs.Teacher)
            .AsNoTracking()
            .AsQueryable();

        if (teacherId.HasValue)
        {
            query = query.Where(cs => cs.TeacherId == teacherId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(cs => cs.SchoolClassId == classId.Value);
        }

        return await query
            .OrderBy(cs => cs.SchoolClass.Name)
            .ThenBy(cs => cs.Subject.Name)
            .Select(cs => new ClassSubjectDto
            {
                Id = cs.Id,
                SchoolClassId = cs.SchoolClassId,
                SchoolClassName = $"{cs.SchoolClass.Name} {cs.SchoolClass.Section}".Trim(),
                SubjectId = cs.SubjectId,
                SubjectCode = cs.Subject.Code,
                SubjectName = cs.Subject.Name,
                TeacherId = cs.TeacherId,
                TeacherName = cs.Teacher.FullName,
                TeacherEmail = cs.Teacher.Email,
                CreatedAt = cs.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ClassSubjectDto> AssignTeacherAsync(AssignTeacherRequest request)
    {
        var schoolClass = await _context.SchoolClasses.FindAsync(request.SchoolClassId);
        if (schoolClass == null) throw new NotFoundException("School Class not found");

        var subject = await _context.Subjects.FindAsync(request.SubjectId);
        if (subject == null) throw new NotFoundException("Subject not found");

        var teacher = await _context.Users.FindAsync(request.TeacherId);
        if (teacher == null || teacher.Role != UserRole.Teacher)
        {
            throw new AppException("Assigned user must exist and have the Teacher role");
        }

        var existing = await _context.ClassSubjects
            .FirstOrDefaultAsync(cs => cs.SchoolClassId == request.SchoolClassId && cs.SubjectId == request.SubjectId);

        if (existing != null)
        {
            // Reassign to new teacher
            existing.TeacherId = request.TeacherId;
            await _context.SaveChangesAsync();
            return (await GetAllocationsAsync(classId: request.SchoolClassId))
                .First(cs => cs.SubjectId == request.SubjectId);
        }

        var allocation = new ClassSubject
        {
            Id = Guid.NewGuid(),
            SchoolClassId = request.SchoolClassId,
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClassSubjects.Add(allocation);
        await _context.SaveChangesAsync();

        return (await GetAllocationsAsync(classId: request.SchoolClassId))
            .First(cs => cs.SubjectId == request.SubjectId);
    }

    public async Task<bool> RemoveAllocationAsync(Guid id)
    {
        var allocation = await _context.ClassSubjects.FindAsync(id);
        if (allocation == null) throw new NotFoundException("Allocation not found");

        _context.ClassSubjects.Remove(allocation);
        await _context.SaveChangesAsync();
        return true;
    }
}
