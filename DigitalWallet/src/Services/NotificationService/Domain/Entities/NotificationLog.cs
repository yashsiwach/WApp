namespace NotificationService.Domain.Entities;

/// <summary>Persisted record of every notification sent or attempted.</summary>
public class NotificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;      // Email | SMS | Push
    public string Type { get; set; } = string.Empty;         // Welcome | TopUp | Transfer | KYC | Points | Redemption | PaymentFailed
    public string Recipient { get; set; } = string.Empty;    // email address or phone number
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";          // Pending | Sent | Failed
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}
