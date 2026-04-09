namespace RewardsService.Application.Interfaces;

/// <summary>Earns points in response to wallet events — called by MassTransit consumers.</summary>
public interface IPointsEarningService
{
    Task EarnPointsAsync(Guid userId, decimal amount, string triggerType, Guid referenceId, string description);
}
