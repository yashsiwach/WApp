using Microsoft.EntityFrameworkCore.Storage;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of IUnitOfWork that coordinates all wallet repositories under a single DbContext.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly WalletDbContext _db;

    /// <summary>
    /// Provides access to the wallet account repository.
    /// </summary>
    public IWalletAccountRepository WalletAccounts { get; }
    /// <summary>
    /// Provides access to the ledger entry repository.
    /// </summary>
    public ILedgerRepository Ledger { get; }
    /// <summary>
    /// Provides access to the top-up request repository.
    /// </summary>
    public ITopUpRepository TopUps { get; }
    /// <summary>
    /// Provides access to the transfer request repository.
    /// </summary>
    public ITransferRepository Transfers { get; }
    /// <summary>
    /// Provides access to the daily limit tracker repository.
    /// </summary>
    public IDailyLimitRepository DailyLimits { get; }

    /// <summary>
    /// Initializes the unit of work by injecting the DbContext and all repository implementations.
    /// </summary>
    public UnitOfWork(
        WalletDbContext db,
        IWalletAccountRepository walletAccounts,
        ILedgerRepository ledger,
        ITopUpRepository topUps,
        ITransferRepository transfers,
        IDailyLimitRepository dailyLimits)
    {
        _db = db;
        WalletAccounts = walletAccounts;
        Ledger = ledger;
        TopUps = topUps;
        Transfers = transfers;
        DailyLimits = dailyLimits;
    }

    /// <summary>
    /// Starts a new database transaction for atomic multi-step operations.
    /// </summary>
    public Task<IDbContextTransaction> BeginTransactionAsync() =>
        _db.Database.BeginTransactionAsync();

    /// <summary>
    /// Persists all pending changes tracked within the current DbContext.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
