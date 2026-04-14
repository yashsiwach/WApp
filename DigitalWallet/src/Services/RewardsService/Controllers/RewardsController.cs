using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces;
using SharedContracts.DTOs;

namespace RewardsService.Controllers;

/// <summary>
/// API controller exposing rewards account, catalog, transaction, and redemption endpoints.
/// </summary>
[ApiController]
[Route("api/rewards")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IRewardsQueryService _query;
    private readonly IRedemptionService _redemption;

    /// <summary>
    /// Initializes the controller with query and redemption service dependencies.
    /// </summary>
    public RewardsController(IRewardsQueryService query, IRedemptionService redemption)
    {
        _query      = query;
        _redemption = redemption;
    }

    /// <summary>
    /// Extracts and validates the authenticated user's identifier from JWT claims.
    /// </summary>
    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : throw new UnauthorizedAccessException("User identity is invalid.");
    }

    /// <summary>Get the authenticated user's rewards account (balance + tier).</summary>
    [HttpGet("account")]
    public async Task<IActionResult> GetAccount()
    {
        var result = await _query.GetAccountAsync(GetUserId());
        return Ok(ApiResponse<RewardsAccountDto>.Ok(result));
    }

    /// <summary>Get paginated rewards transaction history.</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _query.GetTransactionsAsync(GetUserId(), page, size);
        return Ok(ApiResponse<PaginatedResult<RewardsTransactionDto>>.Ok(result));
    }

    /// <summary>Browse the rewards catalog.</summary>
    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var result = await _query.GetCatalogAsync();
        return Ok(ApiResponse<List<CatalogItemDto>>.Ok(result));
    }

    /// <summary>Redeem points for a catalog item.</summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto request)
    {
        var result = await _redemption.RedeemAsync(GetUserId(), request);
        return Ok(ApiResponse<RedeemResponseDto>.Ok(result, "Redemption successful."));
    }

    /// <summary>Get the authenticated user's redemption history.</summary>
    [HttpGet("redemptions")]
    public async Task<IActionResult> GetRedemptions()
    {
        var result = await _query.GetRedemptionsAsync(GetUserId());
        return Ok(ApiResponse<List<RedemptionDto>>.Ok(result));
    }
}
