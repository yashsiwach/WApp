using RewardsService.Application.DTOs;
using SharedContracts.DTOs;

namespace RewardsService.Application.Interfaces;

public interface IRewardsService
{
    Task<RewardsAccountDto> GetAccountAsync(Guid userId);
    Task<PaginatedResult<RewardsTransactionDto>> GetTransactionsAsync(Guid userId, int page, int size);
    Task<List<CatalogItemDto>> GetCatalogAsync();
    Task<RedeemResponseDto> RedeemAsync(Guid userId, RedeemRequestDto request);
    Task<List<RedemptionDto>> GetRedemptionsAsync(Guid userId);

    // Called by MassTransit consumers
    Task EarnPointsAsync(Guid userId, decimal amount, string triggerType, Guid referenceId, string description);
}
