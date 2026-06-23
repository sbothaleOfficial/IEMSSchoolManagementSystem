using System;

namespace IEMS.Core.Entities;

/// <summary>
/// A school-level document (e.g. affiliation / recognition certificate, U-DISE paper, a circular,
/// a government letter, an inspection or audit report). The file bytes are stored in the database so
/// they are covered by the normal backup/restore flow and can be shared to a phone over the LAN.
/// Unlike <see cref="StudentDocument"/> these are not tied to any one student.
/// </summary>
public class SchoolDocument
{
    public int Id { get; set; }

    /// <summary>A category such as "Affiliation / Recognition", "Circular / Notice", "Other".</summary>
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
