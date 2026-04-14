using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that notifies the user by email when a rewards redemption is completed.
/// </summary>
public class RedemptionCompletedConsumer : IConsumer<RedemptionCompleted>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public RedemptionCompletedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the RedemptionCompleted event by sending an email with the redeemed item, points, and coupon code.
    /// </summary>
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
