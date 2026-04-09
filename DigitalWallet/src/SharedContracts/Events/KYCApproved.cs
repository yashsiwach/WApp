namespace SharedContracts.Events;

public record KYCApproved
{
    public Guid UserId { get; init; }
    public Guid DocumentId { get; init; }
    public Guid ReviewedBy { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
