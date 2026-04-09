using RewardsService.Application.DTOs;
using RewardsService.Domain.Entities;

namespace RewardsService.Application.Interfaces.Repositories;

public interface ICatalogRepository
{
    Task<List<CatalogItemDto>> GetAvailableAsync();
    Task<RewardsCatalogItem?> FindByIdAsync(Guid id);
}
