using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly INotificationService _svc;
    public UserRegisteredConsumer(INotificationService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var msg = context.Message;
        await _svc.SendAsync(
            userId: msg.UserId,
            channel: "Email",
            type: "Welcome",
            recipient: msg.Email,
            placeholders: new() { ["Name"] = msg.Email }
        );
    }
}
