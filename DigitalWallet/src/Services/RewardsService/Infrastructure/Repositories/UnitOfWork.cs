using Microsoft.EntityFrameworkCore.Storage;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly RewardsDbContext _db;

    public IRewardsAccountRepository Accounts { get; }
    public IRewardsTransactionRepository Transactions { get; }
    public IEarnRuleRepository EarnRules { get; }
    public ICatalogRepository Catalog { get; }
    public IRedemptionRepository Redemptions { get; }

    public UnitOfWork(
        RewardsDbContext db,
        IRewardsAccountRepository accounts,
        IRewardsTransactionRepository transactions,
        IEarnRuleRepository earnRules,
        ICatalogRepository catalog,
        IRedemptionRepository redemptions)
    {
        _db          = db;
        Accounts     = accounts;
        Transactions = transactions;
        EarnRules    = earnRules;
        Catalog      = catalog;
        Redemptions  = redemptions;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync() =>
        _db.Database.BeginTransactionAsync();

    public Task SaveAsync() => _db.SaveChangesAsync();
}
