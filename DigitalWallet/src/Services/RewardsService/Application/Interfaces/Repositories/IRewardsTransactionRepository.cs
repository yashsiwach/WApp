using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;
using SharedContracts.DTOs;

namespace RewardsService.Application.Interfaces.Repositories;

public interface IRewardsTransactionRepository
{
    Task AddAsync(RewardsTransaction tx);
    Task<PaginatedResult<RewardsTransactionDto>> GetPagedAsync(Guid accountId, int page, int size);
}
