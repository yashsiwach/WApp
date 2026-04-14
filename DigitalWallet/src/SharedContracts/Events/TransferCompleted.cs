namespace SharedContracts.Events;

/// <summary>
/// Event raised when a funds transfer between two wallets is successfully completed.
/// </summary>
public record TransferCompleted
{
    /// <summary>
    /// The unique identifier of the user who sent the funds.
    /// </summary>
    public Guid FromUserId { get; init; }
    /// <summary>
    /// The unique identifier of the user who received the funds.
    /// </summary>
    public Guid ToUserId { get; init; }
    /// <summary>
    /// The unique identifier of the sender's wallet that was debited.
    /// </summary>
    public Guid FromWalletId { get; init; }
    /// <summary>
    /// The unique identifier of the recipient's wallet that was credited.
    /// </summary>
    public Guid ToWalletId { get; init; }
    /// <summary>
    /// The unique identifier of the transfer transaction record.
    /// </summary>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// The monetary amount transferred between the wallets.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// The currency code for the transferred amount.
    /// </summary>
    public string Currency { get; init; } = "INR";
    /// <summary>
    /// The idempotency key used to prevent duplicate processing.
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the transfer was completed.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
