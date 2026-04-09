namespace SharedContracts.Events;

public record PaymentFailed
{
    public Guid UserId { get; init; }
    public Guid WalletId { get; init; }
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string TransactionType { get; init; } = string.Empty; // TopUp or Transfer
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
