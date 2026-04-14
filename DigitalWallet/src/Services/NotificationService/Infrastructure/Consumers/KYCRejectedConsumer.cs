using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that sends a KYC-rejected notification email including the rejection reason.
/// </summary>
public class KYCRejectedConsumer : IConsumer<KYCRejected>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public KYCRejectedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the KYCRejected event by dispatching a KYCRejected notification with the rejection reason.
    /// </summary>
    public async Task Consume(ConsumeContext<KYCRejected> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "KYCRejected",
            recipient: $"user+{msg.UserId}@digitalwallet.app",
            placeholders: new()
            {
                ["Name"]   = "Valued Customer",
                ["Reason"] = msg.Reason
            }
        );
    }
}
