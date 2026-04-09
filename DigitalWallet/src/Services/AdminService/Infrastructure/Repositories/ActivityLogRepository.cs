using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AdminDbContext _db;
    public ActivityLogRepository(AdminDbContext db) => _db = db;

    public async Task AddAsync(AdminActivityLog log) =>
        await _db.ActivityLogs.AddAsync(log);

    public Task<int> CountPendingKYCAsync() =>
        _db.KYCReviews.CountAsync(k => k.Status == "Pending");

    public Task<int> CountKYCByStatusTodayAsync(string status)
    {
        var today    = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return _db.KYCReviews.CountAsync(k => k.Status == status && k.ReviewedAt >= today && k.ReviewedAt < tomorrow);
    }

    public Task<int> CountAdminActionsTodayAsync()
    {
        var today    = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return _db.ActivityLogs.CountAsync(a => a.CreatedAt >= today && a.CreatedAt < tomorrow);
    }

    public Task SaveAsync() => _db.SaveChangesAsync();
}
