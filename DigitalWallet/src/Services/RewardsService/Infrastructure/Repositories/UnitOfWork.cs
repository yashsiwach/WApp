using Microsoft.EntityFrameworkCore.Storage;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

/// <summary>
/// Concrete unit-of-work that groups all rewards repositories and delegates persistence to the EF Core context.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly RewardsDbContext _db;

    /// <summary>
    /// Repository for rewards accounts.
    /// </summary>
    public IRewardsAccountRepository Accounts { get; }
    /// <summary>
    /// Repository for rewards transactions.
    /// </summary>
    public IRewardsTransactionRepository Transactions { get; }
    /// <summary>
    /// Repository for earn rules.
    /// </summary>
    public IEarnRuleRepository EarnRules { get; }
    /// <summary>
    /// Repository for catalog items.
    /// </summary>
    public ICatalogRepository Catalog { get; }
    /// <summary>
    /// Repository for redemption records.
    /// </summary>
    public IRedemptionRepository Redemptions { get; }

    /// <summary>
    /// Initializes the unit-of-work with the database context and all repository implementations.
    /// </summary>
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

    /// <summary>
    /// Begins a new database transaction on the underlying EF Core context.
    /// </summary>
    public Task<IDbContextTransaction> BeginTransactionAsync() =>
        _db.Database.BeginTransactionAsync();

    /// <summary>
    /// Saves all pending EF Core changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
