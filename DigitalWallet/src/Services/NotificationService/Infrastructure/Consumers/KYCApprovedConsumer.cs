using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class KYCApprovedConsumer : IConsumer<KYCApproved>
{
    private readonly INotificationService _svc;
    public KYCApprovedConsumer(INotificationService svc) => _svc = svc;

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
