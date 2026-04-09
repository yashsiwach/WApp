using RewardsService.Application.DTOs;
using SharedContracts.DTOs;

namespace RewardsService.Application.Interfaces;

/// <summary>Read-only rewards queries exposed to the user.</summary>
public interface IRewardsQueryService
{
    Task<RewardsAccountDto> GetAccountAsync(Guid userId);
    Task<PaginatedResult<RewardsTransactionDto>> GetTransactionsAsync(Guid userId, int page, int size);
    Task<List<CatalogItemDto>> GetCatalogAsync();
    Task<List<RedemptionDto>> GetRedemptionsAsync(Guid userId);
}
