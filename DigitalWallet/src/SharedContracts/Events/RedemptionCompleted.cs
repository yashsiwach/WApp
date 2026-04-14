namespace SharedContracts.Events;

/// <summary>
/// Event raised when a user successfully redeems points for a reward item.
/// </summary>
public record RedemptionCompleted
{
    /// <summary>
    /// The unique identifier of the user who completed the redemption.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the redemption transaction.
    /// </summary>
    public Guid RedemptionId { get; init; }
    /// <summary>
    /// The display name of the redeemed reward item.
    /// </summary>
    public string ItemName { get; init; } = string.Empty;
    /// <summary>
    /// The category of the redeemed reward item.
    /// </summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>
    /// The number of points deducted for this redemption.
    /// </summary>
    public int PointsDeducted { get; init; }
    /// <summary>
    /// An optional coupon code issued as part of the redemption.
    /// </summary>
    public string? CouponCode { get; init; }
    /// <summary>
    /// The user's remaining points balance after the redemption.
    /// </summary>
    public int RemainingBalance { get; init; }
    /// <summary>
    /// UTC timestamp when the redemption was completed.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
