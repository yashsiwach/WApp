namespace SharedContracts.Events;

/// <summary>
/// Event raised when a wallet top-up transaction is successfully completed.
/// </summary>
public record TopUpCompleted
{
    /// <summary>
    /// The unique identifier of the user who topped up their wallet.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the wallet that was credited.
    /// </summary>
    public Guid WalletId { get; init; }
    /// <summary>
    /// The unique identifier of the top-up transaction record.
    /// </summary>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// The monetary amount added to the wallet.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// The currency code for the transaction amount.
    /// </summary>
    public string Currency { get; init; } = "INR";
    /// <summary>
    /// The payment provider used to process the top-up.
    /// </summary>
    public string Provider { get; init; } = string.Empty;
    /// <summary>
    /// The idempotency key used to prevent duplicate processing.
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the top-up was completed.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
