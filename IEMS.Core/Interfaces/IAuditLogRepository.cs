using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IEMS.Core.Entities;

namespace IEMS.Core.Interfaces;

public interface IAuditLogRepository
{
    /// <summary>
    /// Returns recent audit entries (newest first), optionally filtered by entity type, action,
    /// free-text (user/summary/entity id) and date range. Capped by <paramref name="maxRows"/>.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> QueryAsync(
        string? entityType = null,
        string? action = null,
        string? search = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int maxRows = 500);

    /// <summary>Distinct entity-type values present in the log (for filter dropdowns).</summary>
    Task<IReadOnlyList<string>> GetEntityTypesAsync();
}
