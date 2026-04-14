using SharedContracts.DTOs;
using WalletService.Application.DTOs;

namespace WalletService.Application.Interfaces;

/// <summary>Read-only wallet queries.</summary>
public interface IWalletQueryService
{
    /// <summary>
    /// Retrieves the current balance and status for the specified user's wallet.
    /// </summary>
    Task<BalanceResponse> GetBalanceAsync(Guid userId);
    /// <summary>
    /// Returns a paginated, optionally date-filtered list of transactions for the specified user.
    /// </summary>
    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size, DateTime? from, DateTime? to);
}
