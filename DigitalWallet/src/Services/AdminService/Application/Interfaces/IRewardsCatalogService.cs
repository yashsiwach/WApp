using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Contract for admin management of the rewards catalog.
/// </summary>
public interface IRewardsCatalogService
{
    /// <summary>
    /// Creates a new rewards catalog item and records an admin activity log entry.
    /// </summary>
    Task<CatalogItemAdminDto> CreateAsync(Guid adminId, CreateCatalogItemRequest request);
    /// <summary>
    /// Updates an existing catalog item's details and records an admin activity log entry.
    /// </summary>
    Task<CatalogItemAdminDto> UpdateAsync(Guid adminId, Guid itemId, UpdateCatalogItemRequest request);
    /// <summary>
    /// Soft-deletes a catalog item by marking it unavailable and records an admin activity log entry.
    /// </summary>
    Task DeleteAsync(Guid adminId, Guid itemId);
}
