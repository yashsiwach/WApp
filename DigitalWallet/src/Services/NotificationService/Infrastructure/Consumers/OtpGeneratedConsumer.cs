using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that delivers an OTP code via email when a one-time password is generated.
/// </summary>
public class OtpGeneratedConsumer : IConsumer<OtpGenerated>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public OtpGeneratedConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the OtpGenerated event by sending an OTP email containing the code and expiry time.
    /// </summary>
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
