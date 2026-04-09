namespace NotificationService.Domain.Entities;

/// <summary>HTML/text template for a notification type and channel.</summary>
public class NotificationTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;         // Welcome | TopUp | etc.
    public string Channel { get; set; } = string.Empty;      // Email | SMS
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty; // Supports {{Name}}, {{Amount}} placeholders
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
