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
}
