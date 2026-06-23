using System;

namespace IEMS.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }          // UTC (stored)
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Summary { get; set; }

    /// <summary>Local-time display of the timestamp for the grid.</summary>
    public string FormattedTimestamp => Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
}
