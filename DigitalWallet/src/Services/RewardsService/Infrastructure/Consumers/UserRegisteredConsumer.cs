using MassTransit;
using RewardsService.Application.Interfaces;
using SharedContracts.Events;

namespace RewardsService.Infrastructure.Consumers;

/// <summary>Creates a rewards account when a new user registers.</summary>
public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IPointsEarningService _rewardsService;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IPointsEarningService rewardsService, ILogger<UserRegisteredConsumer> logger)
    {
        _rewardsService = rewardsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Provisioning rewards account for new user {UserId}", msg.UserId);

        // EarnPointsAsync now creates the account if it doesn't exist before checking earn rules.
        // Passing amount=0 and triggerType="Registration" ensures the account is provisioned
        // without awarding any points (no earn rule for "Registration" exists by design).
        // This call is idempotent — duplicate messages will find the existing account and exit cleanly.
        await _rewardsService.EarnPointsAsync(
            userId: msg.UserId,
            amount: 0m,
            triggerType: "Registration",
            referenceId: msg.UserId,
            description: "Account provisioned on registration"
        );
    }
}
