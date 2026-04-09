using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SharedContracts.DTOs;
using WalletService.Application.DTOs;
using WalletService.Application.Interfaces;

namespace WalletService.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletQueryService _query;
    private readonly IWalletCommandService _command;

    public WalletController(IWalletQueryService query, IWalletCommandService command)
    {
        _query   = query;
        _command = command;
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : throw new UnauthorizedAccessException("User identity is invalid.");
    }

    private string GetBearerToken() =>
        Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

    /// <summary>
    /// Get wallet balance for the authenticated user.
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var result = await _query.GetBalanceAsync(GetUserId());
        return Ok(ApiResponse<BalanceResponse>.Ok(result));
    }

    /// <summary>
    /// Top up wallet balance.
    /// </summary>
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequestDto request)
    {
        var result = await _command.TopUpAsync(GetUserId(), request);
        return Ok(ApiResponse<TopUpResponseDto>.Ok(result, "Top-up successful."));
    }

    /// <summary>
    /// Transfer money to another user's wallet.
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
    {
        var result = await _command.TransferAsync(GetUserId(), request, GetBearerToken());
        return Ok(ApiResponse<TransferResponseDto>.Ok(result, "Transfer successful."));
    }

    /// <summary>
    /// Get paginated transaction history.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _query.GetTransactionsAsync(GetUserId(), page, size, from, to);
        return Ok(ApiResponse<PaginatedResult<TransactionDto>>.Ok(result));
    }
}
