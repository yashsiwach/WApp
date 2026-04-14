using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for retrieving active earn rules used to calculate points.
/// </summary>
public interface IEarnRuleRepository
{
    /// <summary>
    /// Returns the active earn rule for the specified trigger type, or null if none exists.
    /// </summary>
    Task<EarnRule?> FindActiveByTriggerAsync(string triggerType);
}
