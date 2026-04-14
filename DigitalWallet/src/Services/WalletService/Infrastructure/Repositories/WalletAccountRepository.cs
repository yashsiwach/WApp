using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IWalletAccountRepository for querying and persisting wallet accounts.
/// </summary>
public class WalletAccountRepository : IWalletAccountRepository
{
    private readonly WalletDbContext _db;
    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public WalletAccountRepository(WalletDbContext db) => _db = db;

    /// <summary>
    /// Returns the wallet associated with the given user ID, or null if not found.
    /// </summary>
    public Task<WalletAccount?> FindByUserIdAsync(Guid userId) =>
        _db.WalletAccounts.FirstOrDefaultAsync(w => w.UserId == userId);

    /// <summary>
    /// Returns the wallet associated with the given email address, or null if not found.
    /// </summary>
    public Task<WalletAccount?> FindByEmailAsync(string email) =>
        _db.WalletAccounts.FirstOrDefaultAsync(w => w.Email == email.ToLower());

    /// <summary>
    /// Returns the wallet with the given primary key, or null if not found.
    /// </summary>
    public Task<WalletAccount?> FindByIdAsync(Guid id) =>
        _db.WalletAccounts.FindAsync(id).AsTask();

    /// <summary>
    /// Stages a new wallet account for insertion on the next SaveAsync call.
    /// </summary>
    public async Task AddAsync(WalletAccount wallet) =>
        await _db.WalletAccounts.AddAsync(wallet);
}
