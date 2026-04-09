using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

public class DailyLimitRepository : IDailyLimitRepository
{
    private readonly WalletDbContext _db;
    public DailyLimitRepository(WalletDbContext db) => _db = db;

    public Task<DailyLimitTracker?> FindByWalletAndDateAsync(Guid walletId, DateTime date) =>
        _db.DailyLimitTrackers.FirstOrDefaultAsync(d => d.WalletId == walletId && d.Date == date);

    public async Task AddAsync(DailyLimitTracker tracker) =>
        await _db.DailyLimitTrackers.AddAsync(tracker);
}
