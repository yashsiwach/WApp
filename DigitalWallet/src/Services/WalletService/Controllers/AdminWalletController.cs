using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;
using WalletService.Application.DTOs;
using WalletService.Infrastructure.Data;

namespace WalletService.Controllers;

[ApiController]
[Route("api/admin/wallet")]
[Authorize(Roles = "Admin")]
public class AdminWalletController : ControllerBase
{
    private readonly WalletDbContext _db;

    public AdminWalletController(WalletDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get aggregated financial statistics across all wallets.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var today    = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalVolume = await _db.LedgerEntries
            .Where(l => l.Type == "CREDIT")
            .SumAsync(l => (decimal?)l.Amount) ?? 0m;

        var totalTransactionCount = await _db.LedgerEntries.CountAsync();

        var todaysVolume = await _db.LedgerEntries
            .Where(l => l.CreatedAt >= today && l.CreatedAt < tomorrow && l.Type == "CREDIT")
            .SumAsync(l => (decimal?)l.Amount) ?? 0m;

        var todaysTransactionCount = await _db.LedgerEntries
            .CountAsync(l => l.CreatedAt >= today && l.CreatedAt < tomorrow);

        var failedTopUps = await _db.TopUpRequests
            .CountAsync(t => t.Status == "Failed");

        var failedTransfers = await _db.TransferRequests
            .CountAsync(t => t.Status == "Failed");

        var avgValue = totalTransactionCount > 0
            ? Math.Round(totalVolume / totalTransactionCount, 2)
            : 0m;

        var stats = new WalletAdminStatsDto
        {
            TotalTransactionCount  = totalTransactionCount,
            TotalVolume            = totalVolume,
            TodaysVolume           = todaysVolume,
            TodaysTransactionCount = todaysTransactionCount,
            FailedTransactions     = failedTopUps + failedTransfers,
            AverageTransactionValue = avgValue,
        };

        return Ok(ApiResponse<WalletAdminStatsDto>.Ok(stats));
    }
}
