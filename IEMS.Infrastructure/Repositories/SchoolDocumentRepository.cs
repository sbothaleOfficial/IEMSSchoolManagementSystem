using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Infrastructure.Repositories;

public class SchoolDocumentRepository : ISchoolDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public SchoolDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SchoolDocument>> GetAllMetadataAsync()
    {
        // Project WITHOUT Data so the (potentially large) file bytes aren't loaded for the list.
        return await _context.SchoolDocuments
            .AsNoTracking()
            .OrderByDescending(d => d.UploadedAt).ThenByDescending(d => d.Id)
            .Select(d => new SchoolDocument
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt,
                UploadedBy = d.UploadedBy
            })
            .ToListAsync();
    }

    public async Task<SchoolDocument?> GetByIdAsync(int id)
        => await _context.SchoolDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<SchoolDocument> AddAsync(SchoolDocument document)
    {
        _context.SchoolDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task DeleteAsync(int id)
    {
        var doc = await _context.SchoolDocuments.FindAsync(id);
        if (doc != null)
        {
            _context.SchoolDocuments.Remove(doc);
            await _context.SaveChangesAsync();
        }
    }
}
