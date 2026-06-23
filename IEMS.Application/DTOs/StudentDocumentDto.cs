using System;

namespace IEMS.Application.DTOs;

public class StudentDocumentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;

    public string FormattedDate => UploadedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public string FormattedSize =>
        FileSize >= 1024 * 1024 ? $"{FileSize / 1024.0 / 1024.0:0.0} MB"
        : FileSize >= 1024 ? $"{FileSize / 1024.0:0} KB"
        : $"{FileSize} B";
}

/// <summary>A document's bytes plus the info needed to open/save it.</summary>
public record StudentDocumentFile(byte[] Data, string FileName, string ContentType);
