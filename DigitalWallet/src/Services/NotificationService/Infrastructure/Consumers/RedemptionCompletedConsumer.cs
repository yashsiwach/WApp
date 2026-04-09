using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class RedemptionCompletedConsumer : IConsumer<RedemptionCompleted>
{
    private readonly INotificationService _svc;
    public RedemptionCompletedConsumer(INotificationService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<RedemptionCompleted> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "Redemption",
            recipient: $"user+{msg.UserId}@digitalwallet.app",
            placeholders: new()
            {
                ["Name"]   = "Valued Customer",
                ["Item"]   = msg.ItemName,
                ["Points"] = msg.PointsDeducted.ToString(),
                ["Code"]   = msg.CouponCode ?? "N/A"
            }
        );
    }
}
