using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

public interface ITransferRepository
{
    Task<TransferRequest?> FindByIdempotencyKeyAsync(string key);
    Task AddAsync(TransferRequest request);
}
