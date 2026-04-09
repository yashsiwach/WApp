using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>Sends a confirmation email to the user when they open a new support ticket.</summary>
public class TicketCreatedConsumer : IConsumer<TicketCreated>
{
    private readonly INotificationService _notifications;
    private readonly ILogger<TicketCreatedConsumer> _logger;

    public TicketCreatedConsumer(INotificationService notifications, ILogger<TicketCreatedConsumer> logger)
    {
        _notifications = notifications;
        _logger        = logger;
    }

    public async Task Consume(ConsumeContext<TicketCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Sending TicketCreated notification to {Email} for ticket {TicketNumber}", msg.UserEmail, msg.TicketNumber);

        await _notifications.SendAsync(
            userId:       msg.UserId,
            channel:      "Email",
            type:         "TicketCreated",
            recipient:    msg.UserEmail,
            placeholders: new Dictionary<string, string>
            {
                ["TicketNumber"] = msg.TicketNumber,
                ["Subject"]      = msg.Subject,
                ["Category"]     = msg.Category
            });
    }
}
