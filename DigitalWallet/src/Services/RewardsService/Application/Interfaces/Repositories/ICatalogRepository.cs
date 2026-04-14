using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for reading and locating rewards catalog items.
/// </summary>
public interface ICatalogRepository
{
    /// <summary>
    /// Returns all catalog items that are currently marked as available.
    /// </summary>
    Task<List<CatalogItemDto>> GetAvailableAsync();
    /// <summary>
    /// Returns the catalog item with the specified identifier, or null if not found.
    /// </summary>
    Task<RewardsCatalogItem?> FindByIdAsync(Guid id);
}
