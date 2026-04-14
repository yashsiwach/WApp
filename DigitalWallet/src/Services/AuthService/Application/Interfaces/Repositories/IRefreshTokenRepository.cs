using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

/// <summary>
/// Data-access contract for refresh token persistence and lookup operations.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Retrieves an active, non-revoked refresh token by its string value, checking cache first.
    /// </summary>
    Task<RefreshToken?> FindActiveByTokenAsync(string token);
    /// <summary>
    /// Retrieves an active refresh token scoped to a specific user, checking cache first.
    /// </summary>
    Task<RefreshToken?> FindActiveByUserAndTokenAsync(Guid userId, string token);
    /// <summary>
    /// Stages a new refresh token for storage and caches it for fast lookup.
    /// </summary>
    Task AddAsync(RefreshToken token);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
