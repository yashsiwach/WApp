using Microsoft.EntityFrameworkCore.Storage;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WalletDbContext _db;

    public IWalletAccountRepository WalletAccounts { get; }
    public ILedgerRepository Ledger { get; }
    public ITopUpRepository TopUps { get; }
    public ITransferRepository Transfers { get; }
    public IDailyLimitRepository DailyLimits { get; }

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

    public Task<IDbContextTransaction> BeginTransactionAsync() =>
        _db.Database.BeginTransactionAsync();

    public Task SaveAsync() => _db.SaveChangesAsync();
}
