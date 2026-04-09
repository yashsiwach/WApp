using Microsoft.EntityFrameworkCore;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly RewardsDbContext _db;
    public CatalogRepository(RewardsDbContext db) => _db = db;

    public Task<List<CatalogItemDto>> GetAvailableAsync() =>
        _db.CatalogItems
            .Where(c => c.IsAvailable)
            .OrderBy(c => c.PointsCost)
            .Select(c => new CatalogItemDto
            {
                Id            = c.Id,
                Name          = c.Name,
                Description   = c.Description,
                PointsCost    = c.PointsCost,
                Category      = c.Category,
                IsAvailable   = c.IsAvailable,
                StockQuantity = c.StockQuantity
            })
            .ToListAsync();

    public Task<RewardsCatalogItem?> FindByIdAsync(Guid id) =>
        _db.CatalogItems.FindAsync(id).AsTask();
}
