namespace SharedContracts.Events;

public record OtpGenerated
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
