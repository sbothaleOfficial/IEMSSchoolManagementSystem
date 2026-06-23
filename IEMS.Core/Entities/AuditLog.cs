using System;

namespace IEMS.Core.Entities;

/// <summary>
/// An immutable record of a data change (who changed what, when). Written automatically by the
/// SaveChanges audit interceptor for every insert/update/delete except audit rows themselves.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    /// <summary>When the change was saved (UTC).</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Username of the signed-in user who made the change ("system" if none).</summary>
    public string UserName { get; set; } = "system";

    /// <summary>"Added", "Modified" or "Deleted".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>The entity/table affected, e.g. "Student", "FeePayment".</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected row (as text; "(new)" until assigned).</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Human-readable summary of what changed (e.g. changed field names, or a label).</summary>
    public string? Summary { get; set; }
}
