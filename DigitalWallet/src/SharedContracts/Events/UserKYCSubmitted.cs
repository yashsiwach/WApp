namespace SharedContracts.Events;

/// <summary>
/// Event raised when a user submits their KYC document for review.
/// </summary>
public record UserKYCSubmitted
{
    /// <summary>
    /// The unique identifier of the user submitting the KYC document.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the submitted KYC document record.
    /// </summary>
    public Guid DocumentId { get; init; }
    /// <summary>
    /// The type of identity document submitted (e.g., Passport, Aadhaar).
    /// </summary>
    public string DocType { get; init; } = string.Empty;
    /// <summary>
    /// The URL of the uploaded document file in storage.
    /// </summary>
    public string FileUrl { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the submission occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
