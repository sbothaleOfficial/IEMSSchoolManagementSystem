using System;

namespace IEMS.Core.Entities;

/// <summary>
/// A scanned/uploaded document belonging to a student (e.g. Aadhaar, birth certificate, transfer
/// certificate, mark sheet). The file bytes are stored in the database so they are covered by the
/// normal backup/restore flow.
/// </summary>
public class StudentDocument
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public virtual Student? Student { get; set; }

    /// <summary>A label such as "Aadhaar", "Birth Certificate", "Transfer Certificate", "Mark Sheet", "Other".</summary>
    public string DocumentType { get; set; } = "Other";

    /// <summary>Original file name (used as the suggested name when opening/exporting).</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME type, e.g. "image/jpeg", "application/pdf".</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>The file contents.</summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    /// <summary>Username of whoever uploaded it.</summary>
    public string UploadedBy { get; set; } = string.Empty;
}
