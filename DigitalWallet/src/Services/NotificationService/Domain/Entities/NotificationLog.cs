namespace NotificationService.Domain.Entities;

/// <summary>Persisted record of every notification sent or attempted.</summary>
public class NotificationLog
{
    /// <summary>
    /// Unique identifier for this notification log entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the user who triggered or received this notification.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Delivery channel: Email, SMS, or Push.
    /// </summary>
    public string Channel { get; set; } = string.Empty;      // Email | SMS | Push
    /// <summary>
    /// Notification type key (Welcome, TopUp, Transfer, KYC, Points, Redemption, PaymentFailed).
    /// </summary>
    public string Type { get; set; } = string.Empty;         // Welcome | TopUp | Transfer | KYC | Points | Redemption | PaymentFailed
    /// <summary>
    /// Delivery address: email address or phone number depending on Channel.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;    // email address or phone number
    /// <summary>
    /// Subject line of the notification that was sent.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>
    /// Full rendered body content of the notification that was sent.
    /// </summary>
    public string Body { get; set; } = string.Empty;
    /// <summary>
    /// Current delivery status: Pending, Sent, or Failed.
    /// </summary>
    public string Status { get; set; } = "Pending";          // Pending | Sent | Failed
    /// <summary>
    /// Error details populated when the delivery attempt fails.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// UTC timestamp when the notification log record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the notification was successfully delivered, or null if not yet sent.
    /// </summary>
    public DateTime? SentAt { get; set; }
}
