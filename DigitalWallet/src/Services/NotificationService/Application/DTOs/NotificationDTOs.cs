using SharedContracts.DTOs;

namespace NotificationService.Application.DTOs;

/// <summary>
/// Read-only projection of a persisted notification log entry returned to API consumers.
/// </summary>
public record NotificationLogDto
{
    /// <summary>
    /// Unique identifier of the notification log entry.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Delivery channel used for this notification (Email, SMS, Push).
    /// </summary>
    public string Channel { get; init; } = string.Empty;
    /// <summary>
    /// Notification type key (Welcome, TopUp, Transfer, KYC, OTP, etc.).
    /// </summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>
    /// Subject line of the notification message.
    /// </summary>
    public string Subject { get; init; } = string.Empty;
    /// <summary>
    /// Current delivery status: Pending, Sent, or Failed.
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the notification record was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    /// <summary>
    /// UTC timestamp when the notification was successfully sent, or null if not yet sent.
    /// </summary>
    public DateTime? SentAt { get; init; }
}

/// <summary>
/// DTO representation of a notification template returned by the admin templates endpoint.
/// </summary>
public record NotificationTemplateDto
{
    /// <summary>
    /// Unique identifier of the notification template.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Notification type key this template applies to (e.g. Welcome, TopUp).
    /// </summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>
    /// Delivery channel this template is used for (Email, SMS).
    /// </summary>
    public string Channel { get; init; } = string.Empty;
    /// <summary>
    /// Subject line, may contain {{Placeholder}} tokens.
    /// </summary>
    public string Subject { get; init; } = string.Empty;
    /// <summary>
    /// HTML body template with {{Placeholder}} tokens for dynamic substitution.
    /// </summary>
    public string BodyTemplate { get; init; } = string.Empty;
    /// <summary>
    /// Indicates whether this template is active and eligible for use.
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Payload accepted by the update-template endpoint to modify subject, body, and active state.
/// </summary>
public record UpdateTemplateDto
{
    /// <summary>
    /// New subject line for the template, may include {{Placeholder}} tokens.
    /// </summary>
    public string Subject { get; init; } = string.Empty;
    /// <summary>
    /// New HTML body content for the template, may include {{Placeholder}} tokens.
    /// </summary>
    public string BodyTemplate { get; init; } = string.Empty;
    /// <summary>
    /// Whether the template should be active and available for sending.
    /// </summary>
    public bool IsActive { get; init; }
}
