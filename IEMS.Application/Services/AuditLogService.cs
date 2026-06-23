using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEMS.Application.DTOs;
using IEMS.Core.Interfaces;

namespace IEMS.Application.Services;

/// <summary>Read-only access to the audit trail for the admin viewer.</summary>
public class AuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetLogsAsync(
        string? entityType = null,
        string? action = null,
        string? search = null,
        DateTime? fromLocal = null,
        DateTime? toLocal = null,
        int maxRows = 500)
    {
        // The grid filters in local time; the store is UTC.
        var fromUtc = fromLocal?.ToUniversalTime();
        var toUtc = toLocal?.ToUniversalTime();

        var rows = await _repository.QueryAsync(entityType, action, search, fromUtc, toUtc, maxRows);

        return rows.Select(a => new AuditLogDto
        {
            Id = a.Id,
            Timestamp = a.Timestamp,
            UserName = a.UserName,
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Summary = a.Summary
        }).ToList();
    }

    public Task<IReadOnlyList<string>> GetEntityTypesAsync() => _repository.GetEntityTypesAsync();
}
