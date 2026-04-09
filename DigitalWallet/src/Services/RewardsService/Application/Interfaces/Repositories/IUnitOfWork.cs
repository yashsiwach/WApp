using Microsoft.EntityFrameworkCore.Storage;

namespace RewardsService.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IRewardsAccountRepository Accounts { get; }
    IRewardsTransactionRepository Transactions { get; }
    IEarnRuleRepository EarnRules { get; }
    ICatalogRepository Catalog { get; }
    IRedemptionRepository Redemptions { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveAsync();
}
