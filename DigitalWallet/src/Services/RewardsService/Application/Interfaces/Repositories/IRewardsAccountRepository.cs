using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

public interface IRewardsAccountRepository
{
    Task<RewardsAccount?> FindByUserIdAsync(Guid userId);
    Task AddAsync(RewardsAccount account);
}
