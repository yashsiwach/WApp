using Microsoft.EntityFrameworkCore.Storage;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>Wraps DbContext transaction management so services never touch DbContext directly.</summary>
public interface IUnitOfWork
{
    IWalletAccountRepository WalletAccounts { get; }
    ILedgerRepository Ledger { get; }
    ITopUpRepository TopUps { get; }
    ITransferRepository Transfers { get; }
    IDailyLimitRepository DailyLimits { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveAsync();
}
