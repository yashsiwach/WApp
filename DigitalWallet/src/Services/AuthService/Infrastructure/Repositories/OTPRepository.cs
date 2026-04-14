using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.Infrastructure.Repositories;

/// <summary>
/// Cache-backed EF Core implementation of IOTPRepository for OTP log persistence and lookup.
/// </summary>
public class OTPRepository : IOTPRepository
{
    private readonly IMemoryCache _cache;
    private readonly AuthDbContext _db;

    /// <summary>
    /// Initializes the repository with an in-memory cache and the AuthService database context.
    /// </summary>
    public OTPRepository(IMemoryCache cache, AuthDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    /// <summary>
    /// Stores the OTP log in both the in-memory cache and the database.
    /// </summary>
    public Task AddAsync(OTPLog log)
    {
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(log.ExpiresAt);
        _cache.Set($"OTP:{log.Email}:{log.Code}", log, options);
        _db.OTPLogs.Add(log);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a valid, unused OTP matching the email and code, checking the cache before the database.
    /// </summary>
    public async Task<OTPLog?> FindValidAsync(string email, string code)
    {
        if (_cache.TryGetValue($"OTP:{email}:{code}", out OTPLog? log) && log != null)
        {
            if (log.UsedAt == null && log.ExpiresAt > DateTime.UtcNow)
                return log;
            return null;
        }

        // Cache miss — fall back to database
        var dbLog = await _db.OTPLogs
            .Where(o => o.Email == email && o.Code == code && o.UsedAt == null && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.ExpiresAt)
            .FirstOrDefaultAsync();

        if (dbLog != null)
        {
            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(dbLog.ExpiresAt);
            _cache.Set($"OTP:{email}:{code}", dbLog, options);
        }

        return dbLog;
    }

    /// <summary>
    /// Checks whether the email has a successfully used OTP within the specified recency window.
    /// </summary>
    public async Task<bool> HasRecentlyVerifiedOtpAsync(string email, TimeSpan maxAge)
    {
        var verifiedSince = DateTime.UtcNow.Subtract(maxAge);

        return await _db.OTPLogs.AnyAsync(o =>
            o.Email == email &&
            o.UsedAt != null &&
            o.UsedAt >= verifiedSince);
    }

    /// <summary>
    /// Marks an OTP as used directly in the DB using a targeted UPDATE — no EF change tracking needed.
    /// This works correctly regardless of whether the entity came from the in-memory cache or the DB.
    /// </summary>
    public async Task MarkAsUsedAsync(Guid id)
    {
        var usedAt = DateTime.UtcNow;
        await _db.OTPLogs
            .Where(o => o.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.UsedAt, usedAt));
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    public async Task SaveAsync() => await _db.SaveChangesAsync();
}
