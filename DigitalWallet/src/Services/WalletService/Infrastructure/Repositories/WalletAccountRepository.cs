using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

public class WalletAccountRepository : IWalletAccountRepository
{
    private readonly WalletDbContext _db;
    public WalletAccountRepository(WalletDbContext db) => _db = db;

    public Task<WalletAccount?> FindByUserIdAsync(Guid userId) =>
        _db.WalletAccounts.FirstOrDefaultAsync(w => w.UserId == userId);

    public Task<WalletAccount?> FindByEmailAsync(string email) =>
        _db.WalletAccounts.FirstOrDefaultAsync(w => w.Email == email.ToLower());

    public Task<WalletAccount?> FindByIdAsync(Guid id) =>
        _db.WalletAccounts.FindAsync(id).AsTask();

    public async Task AddAsync(WalletAccount wallet) =>
        await _db.WalletAccounts.AddAsync(wallet);
}
