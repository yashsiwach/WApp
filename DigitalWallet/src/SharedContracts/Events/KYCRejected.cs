namespace SharedContracts.Events;

public record KYCRejected
{
    public Guid UserId { get; init; }
    public Guid DocumentId { get; init; }
    public Guid ReviewedBy { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
