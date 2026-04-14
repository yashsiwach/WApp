using MassTransit;
using NotificationService.Application.Interfaces;
using SharedContracts.Events;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that sends a welcome email when a new user completes registration.
/// </summary>
public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly INotificationService _svc;
    /// <summary>
    /// Initializes the consumer with the notification service dependency.
    /// </summary>
    public UserRegisteredConsumer(INotificationService svc) => _svc = svc;

    /// <summary>
    /// Handles the UserRegistered event by sending a Welcome notification to the new user's email.
    /// </summary>
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
