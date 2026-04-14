using Microsoft.EntityFrameworkCore;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;
using SharedContracts.DTOs;

namespace RewardsService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRewardsTransactionRepository for recording and paginating transactions.
/// </summary>
public class RewardsTransactionRepository : IRewardsTransactionRepository
{
    private readonly RewardsDbContext _db;
    /// <summary>
    /// Initializes the repository with the rewards database context.
    /// </summary>
    public RewardsTransactionRepository(RewardsDbContext db) => _db = db;

    /// <summary>
    /// Adds a new rewards transaction entity to the EF tracking context.
    /// </summary>
    public async Task AddAsync(RewardsTransaction tx) =>
        await _db.RewardsTransactions.AddAsync(tx);

    /// <summary>
    /// Returns a paginated, most-recent-first page of transactions for the specified account.
    /// </summary>
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
