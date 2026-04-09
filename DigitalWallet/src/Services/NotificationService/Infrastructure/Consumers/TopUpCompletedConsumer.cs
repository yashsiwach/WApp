using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class TopUpCompletedConsumer : IConsumer<TopUpCompleted>
{
    private readonly INotificationService _svc;
    public TopUpCompletedConsumer(INotificationService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<TopUpCompleted> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "TopUp",
            recipient: $"user+{msg.UserId}@digitalwallet.app", // real: lookup user email
            placeholders: new()
            {
                ["Amount"]  = msg.Amount.ToString("N2"),
                ["Balance"] = "N/A",                           // real: fetch from WalletService or cache
                ["Name"]    = "Valued Customer"
            }
        );
    }
}
