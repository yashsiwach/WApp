using Microsoft.EntityFrameworkCore;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;
using SharedContracts.DTOs;

namespace RewardsService.Infrastructure.Repositories;

public class RewardsTransactionRepository : IRewardsTransactionRepository
{
    private readonly RewardsDbContext _db;
    public RewardsTransactionRepository(RewardsDbContext db) => _db = db;

    public async Task AddAsync(RewardsTransaction tx) =>
        await _db.RewardsTransactions.AddAsync(tx);

    public async Task<PaginatedResult<RewardsTransactionDto>> GetPagedAsync(Guid accountId, int page, int size)
    {
        var query = _db.RewardsTransactions.Where(t => t.RewardsAccountId == accountId);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new RewardsTransactionDto
            {
                Id            = t.Id,
                Type          = t.Type,
                PointsDelta   = t.PointsDelta,
                ReferenceType = t.ReferenceType,
                Description   = t.Description,
                CreatedAt     = t.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<RewardsTransactionDto> { Items = items, Page = page, PageSize = size, TotalCount = total };
    }
}
