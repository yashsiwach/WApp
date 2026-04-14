using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

/// <summary>
/// Data-access contract for KYC document persistence operations.
/// </summary>
public interface IKYCRepository
{
    /// <summary>
    /// Returns true if the user already has a pending document of the given type.
    /// </summary>
    Task<bool> HasPendingAsync(Guid userId, string docType);
    /// <summary>
    /// Looks up a user by their unique identifier, or null if not found.
    /// </summary>
    Task<User?> FindUserByIdAsync(Guid userId);
    /// <summary>
    /// Stages a new KYC document for insertion into the data store.
    /// </summary>
    Task AddAsync(KYCDocument doc);
    /// <summary>
    /// Returns all KYC documents belonging to the specified user, newest first.
    /// </summary>
    Task<List<KYCDocument>> GetByUserIdAsync(Guid userId);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
