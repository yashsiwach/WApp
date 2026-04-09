using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

public interface IWalletAccountRepository
{
    Task<WalletAccount?> FindByUserIdAsync(Guid userId);
    Task<WalletAccount?> FindByEmailAsync(string email);
    Task<WalletAccount?> FindByIdAsync(Guid id);
    Task AddAsync(WalletAccount wallet);
}
