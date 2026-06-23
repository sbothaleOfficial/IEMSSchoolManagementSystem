using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Infrastructure.Repositories;

public class StudentDocumentRepository : IStudentDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StudentDocument>> GetMetadataByStudentAsync(int studentId)
    {
        // Project WITHOUT Data so the (potentially large) file bytes aren't loaded for the list.
        return await _context.StudentDocuments
            .AsNoTracking()
            .Where(d => d.StudentId == studentId)
            .OrderByDescending(d => d.UploadedAt).ThenByDescending(d => d.Id)
            .Select(d => new StudentDocument
            {
                Id = d.Id,
                StudentId = d.StudentId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt,
                UploadedBy = d.UploadedBy
            })
            .ToListAsync();
    }

    public async Task<StudentDocument?> GetByIdAsync(int id)
        => await _context.StudentDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<StudentDocument> AddAsync(StudentDocument document)
    {
        _context.StudentDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task DeleteAsync(int id)
    {
        var doc = await _context.StudentDocuments.FindAsync(id);
        if (doc != null)
        {
            _context.StudentDocuments.Remove(doc);
            await _context.SaveChangesAsync();
        }
    }
}
