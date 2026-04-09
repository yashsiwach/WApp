using Microsoft.EntityFrameworkCore;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

public class RedemptionRepository : IRedemptionRepository
{
    private readonly RewardsDbContext _db;
    public RedemptionRepository(RewardsDbContext db) => _db = db;

    public async Task AddAsync(Redemption redemption) =>
        await _db.Redemptions.AddAsync(redemption);

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
