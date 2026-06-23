using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEMS.Application.DTOs;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;

namespace IEMS.Application.Services;

public class StudentDocumentService
{
    private readonly IStudentDocumentRepository _repository;

    // Document types offered in the UI.
    public static readonly IReadOnlyList<string> DocumentTypes = new[]
    {
        "Aadhaar Card", "Birth Certificate", "Transfer Certificate", "Mark Sheet",
        "Caste Certificate", "Income Certificate", "Photo", "Other"
    };

    public StudentDocumentService(IStudentDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<StudentDocumentDto>> GetDocumentsAsync(int studentId)
    {
        var docs = await _repository.GetMetadataByStudentAsync(studentId);
        return docs.Select(d => new StudentDocumentDto
        {
            Id = d.Id,
            StudentId = d.StudentId,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            UploadedAt = d.UploadedAt,
            UploadedBy = d.UploadedBy
        }).ToList();
    }

    public async Task<StudentDocumentFile?> GetFileAsync(int documentId)
    {
        var doc = await _repository.GetByIdAsync(documentId);
        if (doc == null) return null;
        return new StudentDocumentFile(doc.Data, doc.FileName, doc.ContentType);
    }

    public async Task<StudentDocumentDto> AddDocumentAsync(int studentId, string documentType,
        string fileName, string contentType, byte[] data, string uploadedBy)
    {
        if (data == null || data.Length == 0)
            throw new InvalidOperationException("The document is empty.");

        var doc = new StudentDocument
        {
            StudentId = studentId,
            DocumentType = string.IsNullOrWhiteSpace(documentType) ? "Other" : documentType,
            FileName = string.IsNullOrWhiteSpace(fileName) ? "document" : fileName,
            ContentType = contentType ?? string.Empty,
            Data = data,
            FileSize = data.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy ?? string.Empty
        };

        var saved = await _repository.AddAsync(doc);
        return new StudentDocumentDto
        {
            Id = saved.Id,
            StudentId = saved.StudentId,
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
