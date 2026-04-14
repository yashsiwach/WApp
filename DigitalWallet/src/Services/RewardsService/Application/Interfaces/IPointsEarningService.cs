namespace RewardsService.Application.Interfaces;

/// <summary>Earns points in response to wallet events — called by MassTransit consumers.</summary>
public interface IPointsEarningService
{
    /// <summary>
    /// Awards points to a user based on a wallet event amount and the matching earn rule.
    /// </summary>
    Task EarnPointsAsync(Guid userId, decimal amount, string triggerType, Guid referenceId, string description);
}
