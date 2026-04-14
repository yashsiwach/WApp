using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that emails the user a wallet top-up confirmation when funds are added.
/// </summary>
public class TopUpCompletedConsumer : IConsumer<TopUpCompleted>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public TopUpCompletedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the TopUpCompleted event by sending a top-up confirmation email with the credited amount.
    /// </summary>
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
