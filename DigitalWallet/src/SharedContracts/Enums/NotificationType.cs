namespace SharedContracts.Enums;

/// <summary>
/// Specifies the delivery channel for a user notification.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Notification delivered via email.
    /// </summary>
    Email = 0,
    /// <summary>
    /// Notification delivered via SMS text message.
    /// </summary>
    SMS = 1,
    /// <summary>
    /// Notification delivered via mobile push notification.
    /// </summary>
    Push = 2
}
