namespace SharedContracts.Events;

public record PointsEarned
{
    public Guid UserId { get; init; }
    public Guid RewardsAccountId { get; init; }
    public int Points { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Tier { get; init; } = string.Empty;
    public int NewBalance { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
