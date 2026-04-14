using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;
using WalletService.Application.DTOs;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ILedgerRepository for writing and querying ledger entries.
/// </summary>
public class LedgerRepository : ILedgerRepository
{
    private readonly WalletDbContext _db;
    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public LedgerRepository(WalletDbContext db) => _db = db;

    /// <summary>
    /// Stages a new ledger entry for insertion on the next SaveAsync call.
    /// </summary>
    public async Task AddAsync(LedgerEntry entry) =>
        await _db.LedgerEntries.AddAsync(entry);

    /// <summary>
    /// Returns a paginated, optionally date-filtered transaction history for the specified wallet.
    /// </summary>
    public async Task<PaginatedResult<TransactionDto>> GetPagedAsync(Guid walletId, int page, int size, DateTime? from, DateTime? to)
    {
        // Build the base filter query scoped to the wallet and optional date range
        var query = _db.LedgerEntries.Where(l => l.WalletId == walletId).AsQueryable();

        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)   query = query.Where(l => l.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        // Apply ordering and pagination, then project each entry to a TransactionDto
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(l => new TransactionDto
            {
                Id            = l.Id,
                Type          = l.Type,
                Amount        = l.Amount,
                ReferenceId   = l.ReferenceId,
                ReferenceType = l.ReferenceType,
                Status        =
                    l.ReferenceType == "TopUp"
                        ? _db.TopUpRequests
                            .Where(t => t.Id == l.ReferenceId)
                            .Select(t => t.Status)
                            .FirstOrDefault() ?? "Completed"
                        : l.ReferenceType == "Transfer"
                            ? _db.TransferRequests
                                .Where(t => t.Id == l.ReferenceId)
                                .Select(t => t.Status)
                                .FirstOrDefault() ?? "Completed"
                            : "Completed",
                Description   = l.Description,
                CreatedAt     = l.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<TransactionDto> { Items = items, Page = page, PageSize = size, TotalCount = total };
    }
}
