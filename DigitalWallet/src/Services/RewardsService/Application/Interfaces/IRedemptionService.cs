using RewardsService.Application.DTOs;

namespace RewardsService.Application.Interfaces;

/// <summary>Handles points redemption against the catalog.</summary>
public interface IRedemptionService
{
    /// <summary>
    /// Redeems a catalog item for a user, deducting the required points from their balance.
    /// </summary>
    Task<RedeemResponseDto> RedeemAsync(Guid userId, RedeemRequestDto request);
}
