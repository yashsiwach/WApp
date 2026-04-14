using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that emails the user when loyalty points are credited to their account.
/// </summary>
public class PointsEarnedConsumer : IConsumer<PointsEarned>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public PointsEarnedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the PointsEarned event by sending an email with the earned points, new balance, and tier.
    /// </summary>
    public async Task Consume(ConsumeContext<PointsEarned> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "PointsEarned",
            recipient: $"user+{msg.UserId}@digitalwallet.app",
            placeholders: new()
            {
                ["Name"]    = "Valued Customer",
                ["Points"]  = msg.Points.ToString(),
                ["Balance"] = msg.NewBalance.ToString(),
                ["Tier"]    = msg.Tier
            }
        );
    }
}
