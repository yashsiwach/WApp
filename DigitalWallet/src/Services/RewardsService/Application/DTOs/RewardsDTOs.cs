namespace RewardsService.Application.DTOs;

// ── Account ──
public record RewardsAccountDto
{
    public Guid Id { get; init; }
    public int PointsBalance { get; init; }
    public string Tier { get; init; } = string.Empty;
    public int LifetimePoints { get; init; }
}

// ── Transaction History ──
public record RewardsTransactionDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public int PointsDelta { get; init; }
    public string ReferenceType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

// ── Catalog ──
public record CatalogItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int PointsCost { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public int StockQuantity { get; init; }
}

// ── Redemption ──
public record RedeemRequestDto
{
    public Guid CatalogItemId { get; init; }
}

public record RedeemResponseDto
{
    public Guid RedemptionId { get; init; }
    public int PointsSpent { get; init; }
    public int RemainingBalance { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FulfillmentCode { get; init; }
}

public record RedemptionDto
{
    public Guid Id { get; init; }
    public string CatalogItemName { get; init; } = string.Empty;
    public int PointsSpent { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FulfillmentCode { get; init; }
    public DateTime CreatedAt { get; init; }
}
