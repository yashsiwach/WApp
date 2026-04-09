using MassTransit;
using RewardsService.Application.Interfaces;
using SharedContracts.Events;

namespace RewardsService.Infrastructure.Consumers;

/// <summary>Awards points to sender when a transfer completes.</summary>
public class TransferCompletedConsumer : IConsumer<TransferCompleted>
{
    private readonly IPointsEarningService _rewardsService;
    private readonly ILogger<TransferCompletedConsumer> _logger;

    public TransferCompletedConsumer(IPointsEarningService rewardsService, ILogger<TransferCompletedConsumer> logger)
    {
        _rewardsService = rewardsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TransferCompleted> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing TransferCompleted for user {UserId}, ₹{Amount}", msg.FromUserId, msg.Amount);

        // Only sender earns points
        await _rewardsService.EarnPointsAsync(
            userId: msg.FromUserId,
            amount: msg.Amount,
            triggerType: "Transfer",
            referenceId: msg.TransactionId,
            description: $"Points for transfer of ₹{msg.Amount:N2}"
        );
    }
}
