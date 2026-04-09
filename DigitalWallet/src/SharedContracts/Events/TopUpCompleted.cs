namespace SharedContracts.Events;

public record TopUpCompleted
{
    public Guid UserId { get; init; }
    public Guid WalletId { get; init; }
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "INR";
    public string Provider { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
