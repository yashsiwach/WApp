using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

/// <summary>
/// Data-access contract for OTP log persistence and lookup operations.
/// </summary>
public interface IOTPRepository
{
    /// <summary>
    /// Stages a new OTP log entry for storage, caching it for fast retrieval.
    /// </summary>
    Task AddAsync(OTPLog log);
    /// <summary>
    /// Finds an unexpired, unused OTP matching the email and code, checking cache before the database.
    /// </summary>
    Task<OTPLog?> FindValidAsync(string email, string code);
    /// <summary>
    /// Returns true if the email has a successfully verified OTP within the specified time window.
    /// </summary>
    Task<bool> HasRecentlyVerifiedOtpAsync(string email, TimeSpan maxAge);
    /// <summary>
    /// Records the usage timestamp on the specified OTP entry to prevent reuse.
    /// </summary>
    Task MarkAsUsedAsync(Guid id);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
