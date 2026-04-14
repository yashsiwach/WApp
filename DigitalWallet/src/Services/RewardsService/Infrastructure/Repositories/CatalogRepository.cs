using Microsoft.EntityFrameworkCore;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICatalogRepository for querying rewards catalog items.
/// </summary>
public class CatalogRepository : ICatalogRepository
{
    private readonly RewardsDbContext _db;
    /// <summary>
    /// Initializes the repository with the rewards database context.
    /// </summary>
    public CatalogRepository(RewardsDbContext db) => _db = db;

    /// <summary>
    /// Returns all available catalog items ordered by ascending points cost.
    /// </summary>
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

    /// <summary>
    /// Returns the catalog item entity with the given ID, or null if not found.
    /// </summary>
    public Task<RewardsCatalogItem?> FindByIdAsync(Guid id) => _db.CatalogItems.FindAsync(id).AsTask();
}
