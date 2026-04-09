using SharedContracts.DTOs;
using WalletService.Application.DTOs;

namespace WalletService.Application.Interfaces;

/// <summary>Read-only wallet queries.</summary>
public interface IWalletQueryService
{
    Task<BalanceResponse> GetBalanceAsync(Guid userId);
    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size, DateTime? from, DateTime? to);
}
