using RewardsService.Application.DTOs;

namespace RewardsService.Application.Interfaces;

/// <summary>Handles points redemption against the catalog.</summary>
public interface IRedemptionService
{
    Task<RedeemResponseDto> RedeemAsync(Guid userId, RedeemRequestDto request);
}
