using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;
using SharedContracts.DTOs;

namespace RewardsService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for recording and paginating rewards transactions.
/// </summary>
public interface IRewardsTransactionRepository
{
    /// <summary>
    /// Persists a new rewards transaction to the data store.
    /// </summary>
    Task AddAsync(RewardsTransaction tx);
    /// <summary>
    /// Returns a paginated, most-recent-first list of transactions for the specified account.
    /// </summary>
    Task<PaginatedResult<RewardsTransactionDto>> GetPagedAsync(Guid accountId, int page, int size);
}
