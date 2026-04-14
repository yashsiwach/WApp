using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;

namespace RewardsService.Application.Mappers;

/// <summary>Single responsibility: maps RewardsService domain objects to DTOs.</summary>
public static class RewardsMapper
{
    /// <summary>
    /// Maps a RewardsAccount entity to its DTO representation.
    /// </summary>
    public static RewardsAccountDto ToDto(RewardsAccount account) => new()
    {
        Id             = account.Id,
        PointsBalance  = account.PointsBalance,
        Tier           = account.Tier,
        LifetimePoints = account.LifetimePoints
    };

    /// <summary>
    /// Maps a Redemption entity and the associated item name to its DTO representation.
    /// </summary>
    public static RedemptionDto ToDto(Redemption redemption, string itemName) => new()
    {
        Id              = redemption.Id,
        CatalogItemName = itemName,
        PointsSpent     = redemption.PointsSpent,
        Status          = redemption.Status,
        FulfillmentCode = redemption.FulfillmentCode,
        CreatedAt       = redemption.CreatedAt
    };

    /// <summary>
    /// Maps a completed Redemption entity and remaining balance to a redeem response DTO.
    /// </summary>
    public static RedeemResponseDto ToRedeemResponse(Redemption redemption, int remainingBalance) => new()
    {
        RedemptionId     = redemption.Id,
        PointsSpent      = redemption.PointsSpent,
        RemainingBalance = remainingBalance,
        Status           = redemption.Status,
        FulfillmentCode  = redemption.FulfillmentCode
    };

    /// <summary>
    /// Maps a RewardsCatalogItem entity to its DTO representation.
    /// </summary>
    public static CatalogItemDto ToDto(RewardsCatalogItem item) => new()
    {
        Id            = item.Id,
        Name          = item.Name,
        Description   = item.Description,
        PointsCost    = item.PointsCost,
        Category      = item.Category,
        IsAvailable   = item.IsAvailable,
        StockQuantity = item.StockQuantity
    };
}
