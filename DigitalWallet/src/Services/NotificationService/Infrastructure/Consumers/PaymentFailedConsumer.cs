using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that notifies the user by email when a payment or transfer attempt fails.
/// </summary>
public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public PaymentFailedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the PaymentFailed event by sending a failure notification with the amount, type, and reason.
    /// </summary>
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
