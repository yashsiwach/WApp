namespace SharedContracts.Events;

public record TransferCompleted
{
    public Guid FromUserId { get; init; }
    public Guid ToUserId { get; init; }
    public Guid FromWalletId { get; init; }
    public Guid ToWalletId { get; init; }
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "INR";
    public string IdempotencyKey { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
