using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that sends a transfer-confirmation email to the sender when a transfer completes.
/// </summary>
public class TransferCompletedConsumer : IConsumer<TransferCompleted>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public TransferCompletedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the TransferCompleted event by notifying the sender with the transferred amount.
    /// </summary>
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
