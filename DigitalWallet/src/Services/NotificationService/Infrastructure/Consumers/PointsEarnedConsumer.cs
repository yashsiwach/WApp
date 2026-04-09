using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class PointsEarnedConsumer : IConsumer<PointsEarned>
{
    private readonly INotificationService _svc;
    public PointsEarnedConsumer(INotificationService svc) => _svc = svc;

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
