using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IRewardsCatalogService
{
    Task<CatalogItemAdminDto> CreateAsync(Guid adminId, CreateCatalogItemRequest request);
}
