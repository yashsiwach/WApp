namespace RewardsService.Application.DTOs;

// ── Account ──
/// <summary>
/// Data transfer object representing a user's rewards account summary.
/// </summary>
public record RewardsAccountDto
{
    /// <summary>
    /// Unique identifier of the rewards account.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Current redeemable points balance.
    /// </summary>
    public int PointsBalance { get; init; }
    /// <summary>
    /// Membership tier name (Bronze, Silver, Gold, Platinum).
    /// </summary>
    public string Tier { get; init; } = string.Empty;
    /// <summary>
    /// Total points ever earned, used for tier calculation.
    /// </summary>
    public int LifetimePoints { get; init; }
}

// ── Transaction History ──
/// <summary>
/// Data transfer object representing a single rewards transaction in history.
/// </summary>
public record RewardsTransactionDto
{
    /// <summary>
    /// Unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Transaction type (EARN, REDEEM, EXPIRE, ADJUST).
    /// </summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>
    /// Points change — positive for earned, negative for spent or expired.
    /// </summary>
    public int PointsDelta { get; init; }
    /// <summary>
    /// Category of the originating event (TopUp, Transfer, Redemption).
    /// </summary>
    public string ReferenceType { get; init; } = string.Empty;
    /// <summary>
    /// Human-readable description of what triggered this transaction.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the transaction was recorded.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

// ── Catalog ──
/// <summary>
/// Data transfer object representing a redeemable item in the rewards catalog.
/// </summary>
public record CatalogItemDto
{
    /// <summary>
    /// Unique identifier of the catalog item.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Display name of the catalog item.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// Detailed description of the catalog item.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// Number of points required to redeem this item.
    /// </summary>
    public int PointsCost { get; init; }
    /// <summary>
    /// Item category (Voucher, Cashback, Gift).
    /// </summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>
    /// Whether the item is currently available for redemption.
    /// </summary>
    public bool IsAvailable { get; init; }
    /// <summary>
    /// Remaining stock quantity; -1 indicates unlimited stock.
    /// </summary>
    public int StockQuantity { get; init; }
}

// ── Redemption ──
/// <summary>
/// Request payload specifying which catalog item the user wants to redeem.
/// </summary>
public record RedeemRequestDto
{
    /// <summary>
    /// Identifier of the catalog item to redeem.
    /// </summary>
    public Guid CatalogItemId { get; init; }
}

/// <summary>
/// Response returned after a successful redemption, including the fulfillment code.
/// </summary>
public record RedeemResponseDto
{
    /// <summary>
    /// Unique identifier of the newly created redemption record.
    /// </summary>
    public Guid RedemptionId { get; init; }
    /// <summary>
    /// Points deducted for this redemption.
    /// </summary>
    public int PointsSpent { get; init; }
    /// <summary>
    /// Points balance remaining after the redemption.
    /// </summary>
    public int RemainingBalance { get; init; }
    /// <summary>
    /// Redemption status (Pending, Completed, Cancelled).
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// Fulfillment code (voucher or gift card code) delivered to the user.
    /// </summary>
    public string? FulfillmentCode { get; init; }
}

/// <summary>
/// Data transfer object representing a past redemption in a user's history.
/// </summary>
public record RedemptionDto
{
    /// <summary>
    /// Unique identifier of the redemption record.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Name of the catalog item that was redeemed.
    /// </summary>
    public string CatalogItemName { get; init; } = string.Empty;
    /// <summary>
    /// Points spent on this redemption.
    /// </summary>
    public int PointsSpent { get; init; }
    /// <summary>
    /// Current status of the redemption (Pending, Completed, Cancelled).
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// Fulfillment code issued at the time of redemption, if applicable.
    /// </summary>
    public string? FulfillmentCode { get; init; }
    /// <summary>
    /// UTC timestamp when the redemption was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
