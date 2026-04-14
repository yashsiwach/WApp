using RewardsService.Application.DTOs;
using SharedContracts.DTOs;

namespace RewardsService.Application.Interfaces;

/// <summary>
/// Unified rewards service interface combining query, redemption, and points-earning operations.
/// </summary>
public interface IRewardsService
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
    /// Redeems a catalog item for a user, deducting the required points from their balance.
    /// </summary>
    Task<RedeemResponseDto> RedeemAsync(Guid userId, RedeemRequestDto request);
    /// <summary>
    /// Returns the full redemption history for the specified user.
    /// </summary>
    Task<List<RedemptionDto>> GetRedemptionsAsync(Guid userId);

    // Called by MassTransit consumers
    /// <summary>
    /// Awards points to a user based on a wallet event amount and the matching earn rule.
    /// </summary>
    Task EarnPointsAsync(Guid userId, decimal amount, string triggerType, Guid referenceId, string description);
}
