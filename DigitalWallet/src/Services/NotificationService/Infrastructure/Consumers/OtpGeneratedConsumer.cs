using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class OtpGeneratedConsumer : IConsumer<OtpGenerated>
{
    private readonly INotificationService _svc;
    public OtpGeneratedConsumer(INotificationService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<OtpGenerated> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: Guid.Empty,
            channel: "Email",
            type: "OTP",
            recipient: msg.Email,
            placeholders: new()
            {
                ["Name"] = msg.Email,
                ["Code"] = msg.Code,
                ["ExpiresAt"] = msg.ExpiresAt.ToString("u")
            }
        );
    }
}
