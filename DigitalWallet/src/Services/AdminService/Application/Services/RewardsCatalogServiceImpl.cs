using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;

namespace AdminService.Application.Services;

public class RewardsCatalogServiceImpl : IRewardsCatalogService
{
    private readonly RewardsAdminDbContext _rewardsDb;
    private readonly IActivityLogRepository _logs;

    public RewardsCatalogServiceImpl(RewardsAdminDbContext rewardsDb, IActivityLogRepository logs)
    {
        _rewardsDb = rewardsDb;
        _logs = logs;
    }

    public async Task<CatalogItemAdminDto> CreateAsync(Guid adminId, CreateCatalogItemRequest request)
    {
        if (request.PointsCost <= 0)
            throw new InvalidOperationException("PointsCost must be greater than 0.");

        var item = new RewardsCatalogItem
        {
            Name = request.Name,
            Description = request.Description,
            PointsCost = request.PointsCost,
            Category = request.Category,
            IsAvailable = request.IsAvailable,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _rewardsDb.CatalogItems.AddAsync(item);
        await _rewardsDb.SaveChangesAsync();

        await _logs.AddAsync(new AdminActivityLog
        {
            AdminUserId = adminId,
            Action = "RewardsCatalogCreated",
            TargetType = "RewardsCatalogItem",
            TargetId = item.Id,
            Details = $"Created rewards catalog item '{item.Name}'"
        });
        await _logs.SaveAsync();

        return new CatalogItemAdminDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            PointsCost = item.PointsCost,
            Category = item.Category,
            IsAvailable = item.IsAvailable,
            StockQuantity = item.StockQuantity
        };
    }

    public async Task<CatalogItemAdminDto> UpdateAsync(Guid adminId, Guid itemId, UpdateCatalogItemRequest request)
    {
        if (request.PointsCost <= 0)
            throw new InvalidOperationException("PointsCost must be greater than 0.");

        var item = await _rewardsDb.CatalogItems.FindAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Catalog item with ID {itemId} not found.");

        var oldName = item.Name;
        item.Name = request.Name;
        item.Description = request.Description;
        item.PointsCost = request.PointsCost;
        item.Category = request.Category;
        item.IsAvailable = request.IsAvailable;
        item.UpdatedAt = DateTime.UtcNow;

        _rewardsDb.CatalogItems.Update(item);
        await _rewardsDb.SaveChangesAsync();

        await _logs.AddAsync(new AdminActivityLog
        {
            AdminUserId = adminId,
            Action = "RewardsCatalogUpdated",
            TargetType = "RewardsCatalogItem",
            TargetId = item.Id,
            Details = $"Updated rewards catalog item '{oldName}' → '{item.Name}'"
        });
        await _logs.SaveAsync();

        return new CatalogItemAdminDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            PointsCost = item.PointsCost,
            Category = item.Category,
            IsAvailable = item.IsAvailable,
            StockQuantity = item.StockQuantity
        };
    }

    public async Task DeleteAsync(Guid adminId, Guid itemId)
    {
        var item = await _rewardsDb.CatalogItems.FindAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Catalog item with ID {itemId} not found.");

        if (item.IsAvailable)
        {
            item.IsAvailable = false;
            item.UpdatedAt = DateTime.UtcNow;
            _rewardsDb.CatalogItems.Update(item);
            await _rewardsDb.SaveChangesAsync();
        }

        await _logs.AddAsync(new AdminActivityLog
        {
            AdminUserId = adminId,
            Action = "RewardsCatalogDeleted",
            TargetType = "RewardsCatalogItem",
            TargetId = item.Id,
            Details = $"Soft-deleted rewards catalog item '{item.Name}' (set IsAvailable=false)"
        });
        await _logs.SaveAsync();
    }
}
