using IEMS.Core.Entities;

namespace IEMS.Core.Interfaces
{
    /// <summary>
    /// Transactional persistence for bulk student promotion. Keeps the transaction/unit-of-work
    /// in Infrastructure so BulkPromotionService (Application) doesn't inject the EF DbContext.
    /// </summary>
    public interface IStudentPromotionRepository
    {
        /// <summary>
        /// Atomically persists the promotion: updates the moved students (ClassId/UpdatedAt already
        /// set by the caller) AND inserts their promotion-history rows in a single transaction.
        /// </summary>
        Task PromoteAsync(IEnumerable<Student> studentsToUpdate, IEnumerable<StudentPromotionHistory> historyToAdd);

        /// <summary>Promotion-history rows for a specific from→to class move (used to identify what to roll back).</summary>
        Task<List<StudentPromotionHistory>> GetHistoryAsync(int fromClassId, int toClassId);

        /// <summary>
        /// Atomically reverts the promotion: moves the given students back AND removes the given
        /// history rows in a single transaction.
        /// </summary>
        Task RollbackAsync(IEnumerable<Student> studentsToRevert, IEnumerable<StudentPromotionHistory> historyToRemove);
    }
}
