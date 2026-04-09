using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

public interface IRedemptionRepository
{
    Task AddAsync(Redemption redemption);
    Task<List<RedemptionDto>> GetByAccountIdAsync(Guid accountId);
}
