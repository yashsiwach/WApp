using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly INotificationService _svc;
    public PaymentFailedConsumer(INotificationService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "PaymentFailed",
            recipient: $"user+{msg.UserId}@digitalwallet.app",
            placeholders: new()
            {
                ["Name"]            = "Valued Customer",
                ["Amount"]          = msg.Amount.ToString("N2"),
                ["Reason"]          = msg.Reason,
                ["TransactionType"] = msg.TransactionType
            }
        );
    }
}
