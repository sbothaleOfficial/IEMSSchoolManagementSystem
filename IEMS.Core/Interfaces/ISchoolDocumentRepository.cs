using System.Collections.Generic;
using System.Threading.Tasks;
using IEMS.Core.Entities;

namespace IEMS.Core.Interfaces;

public interface ISchoolDocumentRepository
{
    /// <summary>All school documents, newest first, WITHOUT the file bytes (metadata only).</summary>
    Task<IReadOnlyList<SchoolDocument>> GetAllMetadataAsync();

    /// <summary>A single document including its file bytes (for opening/exporting/sharing).</summary>
    Task<SchoolDocument?> GetByIdAsync(int id);

    Task<SchoolDocument> AddAsync(SchoolDocument document);

    Task DeleteAsync(int id);
}
