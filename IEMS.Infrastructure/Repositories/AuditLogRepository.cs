using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditLog>> QueryAsync(
        string? entityType = null,
        string? action = null,
        string? search = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int maxRows = 500)
    {
        IQueryable<AuditLog> q = _context.Set<AuditLog>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityType))
            q = q.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(action))
            q = q.Where(a => a.Action == action);

        if (fromUtc.HasValue)
            q = q.Where(a => a.Timestamp >= fromUtc.Value);

        if (toUtc.HasValue)
            q = q.Where(a => a.Timestamp <= toUtc.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a =>
                a.UserName.Contains(s) ||
                a.EntityId.Contains(s) ||
                (a.Summary != null && a.Summary.Contains(s)));
        }

        if (maxRows < 1) maxRows = 1;

        return await q.OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id)
            .Take(maxRows)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetEntityTypesAsync()
    {
        return await _context.Set<AuditLog>().AsNoTracking()
            .Select(a => a.EntityType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }
}
