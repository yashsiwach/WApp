using Microsoft.EntityFrameworkCore.Storage;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>Wraps DbContext transaction management so services never touch DbContext directly.</summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Provides access to the wallet account repository.
    /// </summary>
    IWalletAccountRepository WalletAccounts { get; }
    /// <summary>
    /// Provides access to the ledger entry repository.
    /// </summary>
    ILedgerRepository Ledger { get; }
    /// <summary>
    /// Provides access to the top-up request repository.
    /// </summary>
    ITopUpRepository TopUps { get; }
    /// <summary>
    /// Provides access to the transfer request repository.
    /// </summary>
    ITransferRepository Transfers { get; }
    /// <summary>
    /// Provides access to the daily limit tracker repository.
    /// </summary>
    IDailyLimitRepository DailyLimits { get; }

    /// <summary>
    /// Starts a new database transaction for atomic multi-step operations.
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync();
    /// <summary>
    /// Persists all pending changes tracked within the current DbContext.
    /// </summary>
    Task SaveAsync();
}
