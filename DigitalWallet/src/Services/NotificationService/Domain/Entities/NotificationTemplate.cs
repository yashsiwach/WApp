namespace NotificationService.Domain.Entities;

/// <summary>HTML/text template for a notification type and channel.</summary>
public class NotificationTemplate
{
    /// <summary>
    /// Unique identifier for this notification template.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Notification type key this template covers (Welcome, TopUp, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;         // Welcome | TopUp | etc.
    /// <summary>
    /// Delivery channel this template targets: Email or SMS.
    /// </summary>
    public string Channel { get; set; } = string.Empty;      // Email | SMS
    /// <summary>
    /// Subject line pattern; may contain {{Placeholder}} tokens.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>
    /// HTML body pattern supporting {{Name}}, {{Amount}}, and other placeholder tokens.
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty; // Supports {{Name}}, {{Amount}} placeholders
    /// <summary>
    /// Indicates whether this template is active and can be selected for sending.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// UTC timestamp when this template was originally created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the most recent update to this template.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
