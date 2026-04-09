using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class KYCRejectedConsumer : IConsumer<KYCRejected>
{
    private readonly INotificationService _svc;
    public KYCRejectedConsumer(INotificationService svc) => _svc = svc;

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
