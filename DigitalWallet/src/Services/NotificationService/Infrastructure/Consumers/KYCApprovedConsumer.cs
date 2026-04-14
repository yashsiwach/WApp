using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that sends a KYC-approved notification email when KYC is successfully verified.
/// </summary>
public class KYCApprovedConsumer : IConsumer<KYCApproved>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public KYCApprovedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the KYCApproved event by dispatching a KYCApproved notification to the user.
    /// </summary>
    public async Task Consume(ConsumeContext<KYCApproved> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "KYCApproved",
            recipient: $"user+{msg.UserId}@digitalwallet.app",
            placeholders: new() { ["Name"] = "Valued Customer" }
        );
    }
}
