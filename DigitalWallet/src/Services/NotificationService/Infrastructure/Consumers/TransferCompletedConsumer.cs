using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class TransferCompletedConsumer : IConsumer<TransferCompleted>
{
    private readonly INotificationService _svc;
    public TransferCompletedConsumer(INotificationService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<TransferCompleted> context)
    {
        var msg = context.Message;

        // Notify sender
        await _svc.SendAsync(
            userId: msg.FromUserId,
            channel: "Email",
            type: "Transfer",
            recipient: $"user+{msg.FromUserId}@digitalwallet.app",
            placeholders: new()
            {
                ["Name"]    = "Valued Customer",
                ["Amount"]  = msg.Amount.ToString("N2"),
                ["Balance"] = "N/A"
            }
        );
    }
}
