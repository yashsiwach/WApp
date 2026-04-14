using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>
/// Repository for reading and persisting daily spending-limit tracker records.
/// </summary>
public interface IDailyLimitRepository
{
    /// <summary>
    /// Returns the daily limit tracker for the given wallet and calendar date, or null if none exists.
    /// </summary>
    Task<DailyLimitTracker?> FindByWalletAndDateAsync(Guid walletId, DateTime date);
    /// <summary>
    /// Stages a new daily limit tracker for insertion on the next SaveAsync call.
    /// </summary>
    Task AddAsync(DailyLimitTracker tracker);
}
