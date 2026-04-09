using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;
using WalletService.Application.DTOs;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly WalletDbContext _db;
    public LedgerRepository(WalletDbContext db) => _db = db;

    public async Task AddAsync(LedgerEntry entry) =>
        await _db.LedgerEntries.AddAsync(entry);

    public async Task<PaginatedResult<TransactionDto>> GetPagedAsync(Guid walletId, int page, int size, DateTime? from, DateTime? to)
    {
        var query = _db.LedgerEntries.Where(l => l.WalletId == walletId).AsQueryable();

        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)   query = query.Where(l => l.CreatedAt <= to.Value);

        var total = await query.CountAsync();
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
