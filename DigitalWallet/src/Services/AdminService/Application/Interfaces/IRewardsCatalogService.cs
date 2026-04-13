using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IRewardsCatalogService
{
    Task<CatalogItemAdminDto> CreateAsync(Guid adminId, CreateCatalogItemRequest request);
    Task<CatalogItemAdminDto> UpdateAsync(Guid adminId, Guid itemId, UpdateCatalogItemRequest request);
    Task DeleteAsync(Guid adminId, Guid itemId);
}
