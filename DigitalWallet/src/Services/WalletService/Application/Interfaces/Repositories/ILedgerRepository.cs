using SharedContracts.DTOs;
using WalletService.Application.DTOs;
using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>
/// Repository for writing ledger entries and querying paginated transaction history.
/// </summary>
public interface ILedgerRepository
{
    /// <summary>
    /// Stages a new ledger entry for insertion on the next SaveAsync call.
    /// </summary>
    Task AddAsync(LedgerEntry entry);
    /// <summary>
    /// Returns a paginated, optionally date-filtered transaction history for the specified wallet.
    /// </summary>
    Task<PaginatedResult<TransactionDto>> GetPagedAsync(Guid walletId, int page, int size, DateTime? from, DateTime? to);
}
