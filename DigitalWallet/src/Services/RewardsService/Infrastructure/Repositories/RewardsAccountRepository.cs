using Microsoft.EntityFrameworkCore;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRewardsAccountRepository for locating and persisting rewards accounts.
/// </summary>
public class RewardsAccountRepository : IRewardsAccountRepository
{
    private readonly RewardsDbContext _db;
    /// <summary>
    /// Initializes the repository with the rewards database context.
    /// </summary>
    public RewardsAccountRepository(RewardsDbContext db) => _db = db;

    /// <summary>
    /// Returns the rewards account owned by the specified user, or null if not found.
    /// </summary>
    public Task<RewardsAccount?> FindByUserIdAsync(Guid userId) =>
        _db.RewardsAccounts.FirstOrDefaultAsync(a => a.UserId == userId);

    /// <summary>
    /// Adds a new rewards account entity to the EF tracking context.
    /// </summary>
    public async Task AddAsync(RewardsAccount account) =>
        await _db.RewardsAccounts.AddAsync(account);
}
