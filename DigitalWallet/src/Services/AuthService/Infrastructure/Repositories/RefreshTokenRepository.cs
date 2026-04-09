using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMemoryCache _cache;
    private readonly AuthDbContext _db;

    public RefreshTokenRepository(IMemoryCache cache, AuthDbContext db)
    {
        _cache = cache;
        _db = db;
    }

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

    public async Task<RefreshToken?> FindActiveByUserAndTokenAsync(Guid userId, string token)
    {
        if (_cache.TryGetValue($"RefreshToken:{token}", out RefreshToken? rt) && rt != null && !rt.Revoked && rt.UserId == userId)
            return rt;
        // Cache fall database
        return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token && r.UserId == userId && !r.Revoked && r.ExpiresAt > DateTime.UtcNow);
    }

    public Task AddAsync(RefreshToken token)
    {
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(token.ExpiresAt);
        _cache.Set($"RefreshToken:{token.Token}", token, options);
        _db.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public async Task SaveAsync() => await _db.SaveChangesAsync();
}
