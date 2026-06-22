using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Application.DTOs;

namespace IEMS.Application.Services;

public class ClassService
{
    private readonly IClassRepository _classRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;

    public ClassService(IClassRepository classRepository, ITeacherRepository teacherRepository, IStudentRepository studentRepository)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
    }

    public async Task<IEnumerable<ClassDto>> GetAllClassesAsync()
    {
        var classes = await _classRepository.GetAllAsync();
        var classDtos = new List<ClassDto>();

        foreach (var classEntity in classes)
        {
            var teacher = await _teacherRepository.GetByIdAsync(classEntity.TeacherId);
            var students = await _studentRepository.GetStudentsByClassIdAsync(classEntity.Id);

            classDtos.Add(new ClassDto
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                Section = classEntity.Section,
                TeacherId = classEntity.TeacherId,
                TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unassigned",
                StudentCount = students.Count()
            });
        }

        return classDtos;
    }

    public async Task<ClassDto?> GetClassByIdAsync(int id)
    {
        var classEntity = await _classRepository.GetByIdAsync(id);
        if (classEntity == null) return null;

        var teacher = await _teacherRepository.GetByIdAsync(classEntity.TeacherId);
        var students = await _studentRepository.GetStudentsByClassIdAsync(classEntity.Id);

        return new ClassDto
        {
            Id = classEntity.Id,
            Name = classEntity.Name,
            Section = classEntity.Section,
            TeacherId = classEntity.TeacherId,
            TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unassigned",
            StudentCount = students.Count()
        };
    }

    public async Task<Class> AddClassAsync(ClassDto classDto)
    {
        var classEntity = new Class
        {
            Name = classDto.Name,
            Section = classDto.Section,
            TeacherId = classDto.TeacherId
        };

        return await _classRepository.AddAsync(classEntity);
    }

    public async Task UpdateClassAsync(ClassDto classDto)
    {
        var classEntity = await _classRepository.GetByIdAsync(classDto.Id);
        if (classEntity != null)
        {
            classEntity.Name = classDto.Name;
            classEntity.Section = classDto.Section;
            classEntity.TeacherId = classDto.TeacherId;
            classEntity.UpdatedAt = DateTime.UtcNow;

            await _classRepository.UpdateAsync(classEntity);
        }
    }

    public async Task DeleteClassAsync(int id)
    {
        var students = await _studentRepository.GetStudentsByClassIdAsync(id);
        if (students.Any())
        {
            throw new InvalidOperationException("Cannot delete class that has enrolled students. Please move students to other classes first.");
        }

        await _classRepository.DeleteAsync(id);
    }

    public async Task<bool> IsClassNameSectionUniqueAsync(string name, string section, int? excludeClassId = null)
    {
        var allClasses = await _classRepository.GetAllAsync();
        // Look for ANY class (other than the one being edited) with the same name+section.
        // The previous version only inspected the first match, so a genuine duplicate by
        // another class could slip through depending on ordering.
        var duplicate = allClasses.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            c.Section.Equals(section, StringComparison.OrdinalIgnoreCase) &&
            (!excludeClassId.HasValue || c.Id != excludeClassId.Value));

        return duplicate == null;
    }

    public async Task<IEnumerable<ClassDto>> GetClassesByTeacherIdAsync(int teacherId)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);
        var classDtos = new List<ClassDto>();

        foreach (var classEntity in classes)
        {
            var teacher = await _teacherRepository.GetByIdAsync(classEntity.TeacherId);
            var students = await _studentRepository.GetStudentsByClassIdAsync(classEntity.Id);

            classDtos.Add(new ClassDto
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                Section = classEntity.Section,
                TeacherId = classEntity.TeacherId,
                TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unassigned",
                StudentCount = students.Count()
            });
        }

        return classDtos;
    }

    public async Task<ClassDto?> GetClassWithStudentsAsync(int classId)
    {
        var classEntity = await _classRepository.GetClassWithStudentsAsync(classId);
        if (classEntity == null) return null;

        var teacher = await _teacherRepository.GetByIdAsync(classEntity.TeacherId);

        return new ClassDto
        {
            Id = classEntity.Id,
            Name = classEntity.Name,
            Section = classEntity.Section,
            TeacherId = classEntity.TeacherId,
            TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unassigned",
            StudentCount = classEntity.Students?.Count() ?? 0
        };
    }

    public async Task<IEnumerable<TeacherDto>> GetAvailableTeachersAsync()
    {
        var teachers = await _teacherRepository.GetAllAsync();
        var teacherDtos = new List<TeacherDto>();

        foreach (var teacher in teachers)
        {
            var classes = await _classRepository.GetClassesByTeacherIdAsync(teacher.Id);
            teacherDtos.Add(new TeacherDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                EmployeeId = teacher.EmployeeId,
                ClassCount = classes.Count()
            });
        }

        return teacherDtos.OrderBy(t => t.FirstName).ThenBy(t => t.LastName);
    }
}