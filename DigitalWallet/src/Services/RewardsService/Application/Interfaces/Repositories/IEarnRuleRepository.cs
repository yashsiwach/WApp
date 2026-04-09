using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

public interface IEarnRuleRepository
{
    Task<EarnRule?> FindActiveByTriggerAsync(string triggerType);
}
