using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for locating and persisting rewards accounts.
/// </summary>
public interface IRewardsAccountRepository
{
    /// <summary>
    /// Returns the rewards account belonging to the specified user, or null if not found.
    /// </summary>
    Task<RewardsAccount?> FindByUserIdAsync(Guid userId);
    /// <summary>
    /// Adds a new rewards account to the data store.
    /// </summary>
    Task AddAsync(RewardsAccount account);
}
