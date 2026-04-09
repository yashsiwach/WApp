using MassTransit;
using RewardsService.Application.Interfaces;
using SharedContracts.Events;

namespace RewardsService.Infrastructure.Consumers;

/// <summary>Awards points when a user completes a top-up.</summary>
public class TopUpCompletedConsumer : IConsumer<TopUpCompleted>
{
    private readonly IPointsEarningService _rewardsService;
    private readonly ILogger<TopUpCompletedConsumer> _logger;

    public TopUpCompletedConsumer(IPointsEarningService rewardsService, ILogger<TopUpCompletedConsumer> logger)
    {
        _rewardsService = rewardsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TopUpCompleted> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing TopUpCompleted for user {UserId}, ₹{Amount}", msg.UserId, msg.Amount);

        await _rewardsService.EarnPointsAsync(
            userId: msg.UserId,
            amount: msg.Amount,
            triggerType: "TopUp",
            referenceId: msg.TransactionId,
            description: $"Points for top-up of ₹{msg.Amount:N2}"
        );
    }
}
