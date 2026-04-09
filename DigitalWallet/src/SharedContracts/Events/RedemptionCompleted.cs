namespace SharedContracts.Events;

public record RedemptionCompleted
{
    public Guid UserId { get; init; }
    public Guid RedemptionId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int PointsDeducted { get; init; }
    public string? CouponCode { get; init; }
    public int RemainingBalance { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
