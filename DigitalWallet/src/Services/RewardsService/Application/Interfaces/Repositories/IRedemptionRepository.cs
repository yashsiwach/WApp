using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for persisting and querying user redemption records.
/// </summary>
public interface IRedemptionRepository
{
    /// <summary>
    /// Persists a new redemption record to the data store.
    /// </summary>
    Task AddAsync(Redemption redemption);
    /// <summary>
    /// Returns all redemptions associated with the specified rewards account, newest first.
    /// </summary>
    Task<List<RedemptionDto>> GetByAccountIdAsync(Guid accountId);
}
