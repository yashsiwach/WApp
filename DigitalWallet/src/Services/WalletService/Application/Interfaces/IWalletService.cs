using SharedContracts.DTOs;
using WalletService.Application.DTOs;

namespace WalletService.Application.Interfaces;

public interface IWalletService
{
    Task<BalanceResponse> GetBalanceAsync(Guid userId);
    Task<TopUpResponseDto> TopUpAsync(Guid userId, TopUpRequestDto request);
    Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request);
    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size, DateTime? from, DateTime? to);
}
