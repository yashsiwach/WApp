namespace SharedContracts.Events;

public record UserKYCSubmitted
{
    public Guid UserId { get; init; }
    public Guid DocumentId { get; init; }
    public string DocType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
