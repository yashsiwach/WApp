namespace SharedContracts.Events;

/// <summary>
/// Event raised when a payment transaction fails for a user's wallet.
/// </summary>
public record PaymentFailed
{
    /// <summary>
    /// The unique identifier of the user who initiated the payment.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The unique identifier of the wallet involved in the failed payment.
    /// </summary>
    public Guid WalletId { get; init; }
    /// <summary>
    /// The unique identifier of the failed transaction record.
    /// </summary>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// The monetary amount that was attempted in the transaction.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// The reason why the payment failed.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
    /// <summary>
    /// The type of transaction that failed, either TopUp or Transfer.
    /// </summary>
    public string TransactionType { get; init; } = string.Empty; // TopUp or Transfer
    /// <summary>
    /// UTC timestamp when the failure occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
