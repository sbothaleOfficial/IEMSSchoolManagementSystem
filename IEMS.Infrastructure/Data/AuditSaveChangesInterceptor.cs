using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IEMS.Infrastructure.Data;

/// <summary>
/// Writes an <see cref="AuditLog"/> row for every insert/update/delete (except audit rows
/// themselves) in the same transaction as the change. Inserts whose primary key is database
/// generated are back-filled with the real key after the save completes.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserProvider _currentUser;

    // Audit rows for inserts whose key isn't known until after SaveChanges, paired with the
    // entry so we can read the assigned key in SavedChanges. Scoped per context instance.
    private readonly List<(AuditLog Row, EntityEntry Entry)> _pendingInsertKeys = new();
    private bool _isWritingKeyBackfill;

    public AuditSaveChangesInterceptor(ICurrentUserProvider currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        BackfillInsertKeys(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        BackfillInsertKeys(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context == null || _isWritingKeyBackfill)
            return;

        _pendingInsertKeys.Clear();

        // Materialise first: adding audit rows mutates the change tracker.
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog
                        && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        var now = DateTime.UtcNow;
        var user = string.IsNullOrWhiteSpace(_currentUser.UserName) ? "system" : _currentUser.UserName;

        foreach (var entry in entries)
        {
            var entityType = entry.Metadata.ClrType.Name;

            // Skip "modifications" that didn't actually change any stored value.
            if (entry.State == EntityState.Modified)
            {
                var changed = entry.Properties.Where(p => p.IsModified).ToList();
                if (changed.Count == 0)
                    continue;
            }

            var row = new AuditLog
            {
                Timestamp = now,
                UserName = user,
                Action = entry.State switch
                {
                    EntityState.Added => "Added",
                    EntityState.Deleted => "Deleted",
                    _ => "Modified"
                },
                EntityType = entityType,
                EntityId = GetKey(entry),
                Summary = BuildSummary(entry)
            };

            context.Set<AuditLog>().Add(row);

            if (entry.State == EntityState.Added)
                _pendingInsertKeys.Add((row, entry));
        }
    }

    private void BackfillInsertKeys(DbContext? context)
    {
        if (context == null || _pendingInsertKeys.Count == 0)
            return;

        var changed = false;
        foreach (var (row, entry) in _pendingInsertKeys)
        {
            var key = GetKey(entry);
            if (!string.IsNullOrEmpty(key) && key != row.EntityId)
            {
                row.EntityId = key;
                changed = true;
            }
        }
        _pendingInsertKeys.Clear();

        if (changed)
        {
            // Persist the back-filled keys without re-entering the audit logic.
            _isWritingKeyBackfill = true;
            try { context.SaveChanges(); }
            finally { _isWritingKeyBackfill = false; }
        }
    }

    private static string GetKey(EntityEntry entry)
    {
        var keyProps = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProps == null || keyProps.Count == 0)
            return string.Empty;

        var values = keyProps
            .Select(p => entry.Property(p.Name).CurrentValue)
            .Select(v => v?.ToString() ?? string.Empty)
            .ToList();

        // A database-generated int key is 0 until the insert completes.
        if (values.Count == 1 && (values[0] == "0" || values[0].Length == 0))
            return "(new)";

        return string.Join(",", values);
    }

    private static string? BuildSummary(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Modified:
                var changed = entry.Properties
                    .Where(p => p.IsModified)
                    .Select(p => p.Metadata.Name)
                    .ToList();
                return changed.Count == 0 ? null : "Changed: " + string.Join(", ", changed);
            case EntityState.Added:
                return "Record created";
            case EntityState.Deleted:
                return "Record deleted";
            default:
                return null;
        }
    }
}
