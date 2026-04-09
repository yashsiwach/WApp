using SharedContracts.DTOs;
using WalletService.Application.DTOs;
using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

public interface ILedgerRepository
{
    Task AddAsync(LedgerEntry entry);
    Task<PaginatedResult<TransactionDto>> GetPagedAsync(Guid walletId, int page, int size, DateTime? from, DateTime? to);
}
