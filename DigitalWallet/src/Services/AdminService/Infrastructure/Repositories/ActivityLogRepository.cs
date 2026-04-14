using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IActivityLogRepository providing admin activity and KYC count queries.
/// </summary>
public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AdminDbContext _db;
    /// <summary>
    /// Injects the AdminDbContext used for all data access operations.
    /// </summary>
    public ActivityLogRepository(AdminDbContext db) => _db = db;

    /// <summary>
    /// Stages a new activity log entry into the EF change tracker for later persistence.
    /// </summary>
    public async Task AddAsync(AdminActivityLog log) =>
        await _db.ActivityLogs.AddAsync(log);

    /// <summary>
    /// Returns the total count of KYC reviews currently in Pending status.
    /// </summary>
    public Task<int> CountPendingKYCAsync() =>
        _db.KYCReviews.CountAsync(k => k.Status == "Pending");

    /// <summary>
    /// Returns the count of KYC reviews matching the given status that were reviewed within the current UTC day.
    /// </summary>
    public Task<int> CountKYCByStatusTodayAsync(string status)
    {
        // Date range filter: from midnight today (UTC) up to but not including midnight tomorrow
        var today    = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return _db.KYCReviews.CountAsync(k => k.Status == status && k.ReviewedAt >= today && k.ReviewedAt < tomorrow);
    }

    /// <summary>
    /// Returns the count of all admin activity log entries created within the current UTC day.
    /// </summary>
    public Task<int> CountAdminActionsTodayAsync()
    {
        // Date range filter: from midnight today (UTC) up to but not including midnight tomorrow
        var today    = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return _db.ActivityLogs.CountAsync(a => a.CreatedAt >= today && a.CreatedAt < tomorrow);
    }

    /// <summary>
    /// Flushes all pending EF change tracker changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
