namespace SharedContracts.Events;

/// <summary>
/// Event raised when a user's KYC document is approved by a reviewer.
/// </summary>
public record KYCApproved
{
    /// <summary>
    /// The unique identifier of the user whose KYC was approved.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the approved KYC document.
    /// </summary>
    public Guid DocumentId { get; init; }
    /// <summary>
    /// The unique identifier of the admin who reviewed the document.
    /// </summary>
    public Guid ReviewedBy { get; init; }
    /// <summary>
    /// Optional reviewer notes attached to the approval.
    /// </summary>
    public string Notes { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the approval occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
