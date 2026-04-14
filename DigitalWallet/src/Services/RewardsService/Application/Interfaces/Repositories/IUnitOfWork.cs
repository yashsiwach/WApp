using Microsoft.EntityFrameworkCore.Storage;

namespace RewardsService.Application.Interfaces.Repositories;

/// <summary>
/// Unit-of-work interface that groups all rewards repositories and controls database transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Repository for rewards accounts.
    /// </summary>
    IRewardsAccountRepository Accounts { get; }
    /// <summary>
    /// Repository for rewards transactions.
    /// </summary>
    IRewardsTransactionRepository Transactions { get; }
    /// <summary>
    /// Repository for earn rules.
    /// </summary>
    IEarnRuleRepository EarnRules { get; }
    /// <summary>
    /// Repository for catalog items.
    /// </summary>
    ICatalogRepository Catalog { get; }
    /// <summary>
    /// Repository for redemption records.
    /// </summary>
    IRedemptionRepository Redemptions { get; }

    /// <summary>
    /// Begins a new database transaction and returns the transaction handle.
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync();
    /// <summary>
    /// Persists all pending changes tracked in the current context.
    /// </summary>
    Task SaveAsync();
}
