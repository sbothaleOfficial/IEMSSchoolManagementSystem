using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Infrastructure.Data;

public static class DbContextUpdateExtensions
{
    /// <summary>
    /// Applies an update so that ONLY genuinely-changed columns are marked Modified, rather than
    /// <c>DbSet.Update()</c> which marks every column as changed. This keeps the audit trail
    /// accurate (an edit logs just the fields that actually changed) and avoids blindly
    /// overwriting columns the caller never intended to touch.
    /// </summary>
    public static async Task MergeUpdateAsync<T>(this ApplicationDbContext context, T entity, object key)
        where T : class
    {
        var tracked = await context.FindAsync<T>(key);
        if (tracked == null)
        {
            // Row isn't tracked and (per the key) isn't in the database from this context's view:
            // fall back to a normal update so the change is still persisted.
            context.Set<T>().Update(entity);
        }
        else if (!ReferenceEquals(tracked, entity))
        {
            // Detached copy (e.g. built by an edit form): copy scalar values onto the tracked row;
            // EF then marks only the properties that actually differ as Modified.
            context.Entry(tracked).CurrentValues.SetValues(entity);
        }
        // else: caller already mutated the tracked instance - its real changes are detected on save.
    }
}
