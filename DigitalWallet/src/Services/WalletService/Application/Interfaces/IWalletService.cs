using SharedContracts.DTOs;
using WalletService.Application.DTOs;

namespace WalletService.Application.Interfaces;

/// <summary>
/// Unified facade for all wallet operations, combining query and command responsibilities.
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Retrieves the current balance and status for the specified user's wallet.
    /// </summary>
    Task<BalanceResponse> GetBalanceAsync(Guid userId);
    /// <summary>
    /// Adds funds to the specified user's wallet using the provided top-up details.
    /// </summary>
    Task<TopUpResponseDto> TopUpAsync(Guid userId, TopUpRequestDto request);
    /// <summary>
    /// Transfers funds from the specified user's wallet to another wallet.
    /// </summary>
    Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request);
    /// <summary>
    /// Returns a paginated, optionally date-filtered list of transactions for the specified user.
    /// </summary>
    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size, DateTime? from, DateTime? to);
}
