using Microsoft.EntityFrameworkCore;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRedemptionRepository for persisting and querying redemptions.
/// </summary>
public class RedemptionRepository : IRedemptionRepository
{
    private readonly RewardsDbContext _db;
    /// <summary>
    /// Initializes the repository with the rewards database context.
    /// </summary>
    public RedemptionRepository(RewardsDbContext db) => _db = db;

    /// <summary>
    /// Adds a new redemption entity to the EF tracking context.
    /// </summary>
    public async Task AddAsync(Redemption redemption) =>
        await _db.Redemptions.AddAsync(redemption);

    /// <summary>
    /// Returns all redemptions for the given account, newest first, with catalog item names joined.
    /// </summary>
    public Task<List<RedemptionDto>> GetByAccountIdAsync(Guid accountId) =>
        _db.Redemptions
            .Include(r => r.CatalogItem)
            .Where(r => r.RewardsAccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RedemptionDto
            {
                Id              = r.Id,
                CatalogItemName = r.CatalogItem.Name,
                PointsSpent     = r.PointsSpent,
                Status          = r.Status,
                FulfillmentCode = r.FulfillmentCode,
                CreatedAt       = r.CreatedAt
            })
            .ToListAsync();
}
