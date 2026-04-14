using AdminService.Domain.Entities;

namespace AdminService.Application.Interfaces.Repositories;

/// <summary>
/// Contract for persisting and querying admin activity log records.
/// </summary>
public interface IActivityLogRepository
{
    /// <summary>
    /// Stages a new activity log entry for persistence.
    /// </summary>
    Task AddAsync(AdminActivityLog log);
    /// <summary>
    /// Returns the total number of KYC reviews currently in Pending status.
    /// </summary>
    Task<int> CountPendingKYCAsync();
    /// <summary>
    /// Returns the count of KYC reviews with the given status that were reviewed during the current UTC day.
    /// </summary>
    Task<int> CountKYCByStatusTodayAsync(string status);
    /// <summary>
    /// Returns the total number of admin activity log entries recorded during the current UTC day.
    /// </summary>
    Task<int> CountAdminActionsTodayAsync();
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
