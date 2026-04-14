using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IDailyLimitRepository for daily spending-limit tracking.
/// </summary>
public class DailyLimitRepository : IDailyLimitRepository
{
    private readonly WalletDbContext _db;
    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public DailyLimitRepository(WalletDbContext db) => _db = db;

    /// <summary>
    /// Returns the daily limit tracker for the given wallet and calendar date, or null if none exists.
    /// </summary>
    public Task<DailyLimitTracker?> FindByWalletAndDateAsync(Guid walletId, DateTime date) =>
        _db.DailyLimitTrackers.FirstOrDefaultAsync(d => d.WalletId == walletId && d.Date == date);

    /// <summary>
    /// Stages a new daily limit tracker for insertion on the next SaveAsync call.
    /// </summary>
    public async Task AddAsync(DailyLimitTracker tracker) =>
        await _db.DailyLimitTrackers.AddAsync(tracker);
}
