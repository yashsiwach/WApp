using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>Notifies the recipient by email whenever a reply is added to a support ticket.</summary>
public class TicketRepliedConsumer : IConsumer<TicketReplied>
{
    private readonly INotificationService _notifications;
    private readonly ILogger<TicketRepliedConsumer> _logger;

    public TicketRepliedConsumer(INotificationService notifications, ILogger<TicketRepliedConsumer> logger)
    {
        _notifications = notifications;
        _logger        = logger;
    }

    public async Task Consume(ConsumeContext<TicketReplied> context)
    {
        var msg = context.Message;

        // Skip internal admin-to-admin events (no recipient email)
        if (string.IsNullOrWhiteSpace(msg.RecipientEmail)) return;

        _logger.LogInformation("Sending TicketReplied notification to {Email} for ticket {TicketNumber}", msg.RecipientEmail, msg.TicketNumber);

        var type = msg.ReplyAuthorRole == "Admin" ? "TicketAdminReply" : "TicketUserReply";

        await _notifications.SendAsync(
            userId:       msg.RecipientUserId,
            channel:      "Email",
            type:         type,
            recipient:    msg.RecipientEmail,
            placeholders: new Dictionary<string, string>
            {
                ["TicketNumber"] = msg.TicketNumber,
                ["Subject"]      = msg.TicketSubject,
                ["Message"]      = msg.ReplyMessage
            });
    }
}
