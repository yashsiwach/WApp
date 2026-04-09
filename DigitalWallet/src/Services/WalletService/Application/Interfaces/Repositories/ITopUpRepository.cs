using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

public interface ITopUpRepository
{
    Task<TopUpRequest?> FindByIdempotencyKeyAsync(string key);
    Task AddAsync(TopUpRequest request);
}
