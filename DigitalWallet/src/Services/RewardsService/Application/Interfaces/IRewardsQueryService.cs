using RewardsService.Application.DTOs;
using SharedContracts.DTOs;

namespace RewardsService.Application.Interfaces;

/// <summary>Read-only rewards queries exposed to the user.</summary>
public interface IRewardsQueryService
{
    /// <summary>
    /// Retrieves the rewards account details (balance and tier) for the specified user.
    /// </summary>
    Task<RewardsAccountDto> GetAccountAsync(Guid userId);
    /// <summary>
    /// Returns a paginated list of rewards transactions for the specified user.
    /// </summary>
    Task<PaginatedResult<RewardsTransactionDto>> GetTransactionsAsync(Guid userId, int page, int size);
    /// <summary>
    /// Returns all currently available items in the rewards catalog.
    /// </summary>
    Task<List<CatalogItemDto>> GetCatalogAsync();
    /// <summary>
    /// Returns the full redemption history for the specified user.
    /// </summary>
    Task<List<RedemptionDto>> GetRedemptionsAsync(Guid userId);
}
