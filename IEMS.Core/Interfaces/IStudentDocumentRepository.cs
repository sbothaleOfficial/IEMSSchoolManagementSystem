using System.Collections.Generic;
using System.Threading.Tasks;
using IEMS.Core.Entities;

namespace IEMS.Core.Interfaces;

public interface IStudentDocumentRepository
{
    /// <summary>Documents for a student, newest first, WITHOUT the file bytes (metadata only).</summary>
    Task<IReadOnlyList<StudentDocument>> GetMetadataByStudentAsync(int studentId);

    /// <summary>A single document including its file bytes (for opening/exporting).</summary>
    Task<StudentDocument?> GetByIdAsync(int id);

    Task<StudentDocument> AddAsync(StudentDocument document);

    Task DeleteAsync(int id);
}
