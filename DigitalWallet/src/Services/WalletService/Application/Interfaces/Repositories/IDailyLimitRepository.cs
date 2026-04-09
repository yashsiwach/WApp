using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

public interface IDailyLimitRepository
{
    Task<DailyLimitTracker?> FindByWalletAndDateAsync(Guid walletId, DateTime date);
    Task AddAsync(DailyLimitTracker tracker);
}
