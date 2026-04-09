using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;

namespace RewardsService.Application.Mappers;

/// <summary>Single responsibility: maps RewardsService domain objects to DTOs.</summary>
public static class RewardsMapper
{
    public static RewardsAccountDto ToDto(RewardsAccount account) => new()
    {
        Id             = account.Id,
        PointsBalance  = account.PointsBalance,
        Tier           = account.Tier,
        LifetimePoints = account.LifetimePoints
    };

    public static RedemptionDto ToDto(Redemption redemption, string itemName) => new()
    {
        Id              = redemption.Id,
        CatalogItemName = itemName,
        PointsSpent     = redemption.PointsSpent,
        Status          = redemption.Status,
        FulfillmentCode = redemption.FulfillmentCode,
        CreatedAt       = redemption.CreatedAt
    };

    public static RedeemResponseDto ToRedeemResponse(Redemption redemption, int remainingBalance) => new()
    {
        RedemptionId     = redemption.Id,
        PointsSpent      = redemption.PointsSpent,
        RemainingBalance = remainingBalance,
        Status           = redemption.Status,
        FulfillmentCode  = redemption.FulfillmentCode
    };

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
