using Microsoft.EntityFrameworkCore;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

public class RewardsAccountRepository : IRewardsAccountRepository
{
    private readonly RewardsDbContext _db;
    public RewardsAccountRepository(RewardsDbContext db) => _db = db;

    public Task<RewardsAccount?> FindByUserIdAsync(Guid userId) =>
        _db.RewardsAccounts.FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task AddAsync(RewardsAccount account) =>
        await _db.RewardsAccounts.AddAsync(account);
}
