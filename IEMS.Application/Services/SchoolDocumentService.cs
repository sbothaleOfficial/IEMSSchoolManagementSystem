using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEMS.Application.DTOs;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;

namespace IEMS.Application.Services;

public class SchoolDocumentService
{
    private readonly ISchoolDocumentRepository _repository;

    // Document categories offered in the UI.
    public static readonly IReadOnlyList<string> DocumentTypes = new[]
    {
        "Affiliation / Recognition", "U-DISE / Registration", "Circular / Notice",
        "Government Letter", "Inspection Report", "Financial / Audit",
        "Policy / Guidelines", "Photo", "Other"
    };

    public SchoolDocumentService(ISchoolDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SchoolDocumentDto>> GetDocumentsAsync()
    {
        var docs = await _repository.GetAllMetadataAsync();
        return docs.Select(d => new SchoolDocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            UploadedAt = d.UploadedAt,
            UploadedBy = d.UploadedBy
        }).ToList();
    }

    public async Task<SchoolDocumentFile?> GetFileAsync(int documentId)
    {
        var doc = await _repository.GetByIdAsync(documentId);
        if (doc == null) return null;
        return new SchoolDocumentFile(doc.Data, doc.FileName, doc.ContentType);
    }

    public async Task<SchoolDocumentDto> AddDocumentAsync(string documentType,
        string fileName, string contentType, byte[] data, string uploadedBy)
    {
        if (data == null || data.Length == 0)
            throw new InvalidOperationException("The document is empty.");

        var doc = new SchoolDocument
        {
            DocumentType = string.IsNullOrWhiteSpace(documentType) ? "Other" : documentType,
            FileName = string.IsNullOrWhiteSpace(fileName) ? "document" : fileName,
            ContentType = contentType ?? string.Empty,
            Data = data,
            FileSize = data.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy ?? string.Empty
        };

        var saved = await _repository.AddAsync(doc);
        return new SchoolDocumentDto
        {
            Id = saved.Id,
            DocumentType = saved.DocumentType,
            FileName = saved.FileName,
            ContentType = saved.ContentType,
            FileSize = saved.FileSize,
            UploadedAt = saved.UploadedAt,
            UploadedBy = saved.UploadedBy
        };
    }

    public Task DeleteDocumentAsync(int documentId) => _repository.DeleteAsync(documentId);
}
