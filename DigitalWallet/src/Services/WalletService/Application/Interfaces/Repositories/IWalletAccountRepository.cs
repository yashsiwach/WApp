using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>
/// Repository for querying and persisting wallet account records.
/// </summary>
public interface IWalletAccountRepository
{
    /// <summary>
    /// Returns the wallet associated with the given user ID, or null if not found.
    /// </summary>
    Task<WalletAccount?> FindByUserIdAsync(Guid userId);
    /// <summary>
    /// Returns the wallet associated with the given email address, or null if not found.
    /// </summary>
    Task<WalletAccount?> FindByEmailAsync(string email);
    /// <summary>
    /// Returns the wallet with the given primary key, or null if not found.
    /// </summary>
    Task<WalletAccount?> FindByIdAsync(Guid id);
    /// <summary>
    /// Stages a new wallet account for insertion on the next SaveAsync call.
    /// </summary>
    Task AddAsync(WalletAccount wallet);
}
