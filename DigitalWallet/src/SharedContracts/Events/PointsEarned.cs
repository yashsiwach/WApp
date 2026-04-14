namespace SharedContracts.Events;

/// <summary>
/// Event raised when reward points are credited to a user's rewards account.
/// </summary>
public record PointsEarned
{
    /// <summary>
    /// The unique identifier of the user who earned the points.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the rewards account receiving the points.
    /// </summary>
    public Guid RewardsAccountId { get; init; }
    /// <summary>
    /// The number of points earned in this transaction.
    /// </summary>
    public int Points { get; init; }
    /// <summary>
    /// The reason or activity that triggered the points award.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
    /// <summary>
    /// The user's current rewards tier at the time of earning.
    /// </summary>
    public string Tier { get; init; } = string.Empty;
    /// <summary>
    /// The updated points balance after the credit.
    /// </summary>
    public int NewBalance { get; init; }
    /// <summary>
    /// UTC timestamp when the points were earned.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
