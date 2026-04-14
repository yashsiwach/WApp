using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.Infrastructure.Repositories;

/// <summary>
/// Cache-backed EF Core implementation of IRefreshTokenRepository for refresh token persistence and lookup.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMemoryCache _cache;
    private readonly AuthDbContext _db;

    /// <summary>
    /// Initializes the repository with an in-memory cache and the AuthService database context.
    /// </summary>
    public RefreshTokenRepository(IMemoryCache cache, AuthDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    /// <summary>
    /// Retrieves a non-revoked, non-expired refresh token by its value, checking the cache before the database.
    /// </summary>
    public async Task<RefreshToken?> FindActiveByTokenAsync(string token)
    {
        if (_cache.TryGetValue($"RefreshToken:{token}", out RefreshToken? rt) && rt != null && !rt.Revoked)
        {
            if (rt.User == null)
                rt.User = await _db.Users.FindAsync(rt.UserId);
            return rt;
        }

        // Cache miss — fall back to database
        var dbToken = await _db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token == token && !r.Revoked && r.ExpiresAt > DateTime.UtcNow);

        if (dbToken != null)
        {
            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(dbToken.ExpiresAt); 
            _cache.Set($"RefreshToken:{token}", dbToken, options);
        }

        return dbToken;
    }

    /// <summary>
    /// Retrieves an active refresh token scoped to a specific user, checking the cache before the database.
    /// </summary>
    public async Task<RefreshToken?> FindActiveByUserAndTokenAsync(Guid userId, string token)
    {
        if (_cache.TryGetValue($"RefreshToken:{token}", out RefreshToken? rt) && rt != null && !rt.Revoked && rt.UserId == userId)
            return rt;
        // Cache fall database
        return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token && r.UserId == userId && !r.Revoked && r.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Stores a new refresh token in both the in-memory cache and the database.
    /// </summary>
    public Task AddAsync(RefreshToken token)
    {
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(token.ExpiresAt);
        _cache.Set($"RefreshToken:{token.Token}", token, options);
        _db.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    public async Task SaveAsync() => await _db.SaveChangesAsync();
}
