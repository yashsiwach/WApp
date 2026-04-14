namespace SharedContracts.Events;

/// <summary>
/// Event raised when a user's KYC document is rejected by a reviewer.
/// </summary>
public record KYCRejected
{
    /// <summary>
    /// The unique identifier of the user whose KYC was rejected.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the rejected KYC document.
    /// </summary>
    public Guid DocumentId { get; init; }
    /// <summary>
    /// The unique identifier of the admin who reviewed the document.
    /// </summary>
    public Guid ReviewedBy { get; init; }
    /// <summary>
    /// The reason provided by the reviewer for the rejection.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the rejection occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
