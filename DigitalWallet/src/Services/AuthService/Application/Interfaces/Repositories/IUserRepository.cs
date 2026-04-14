using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

/// <summary>
/// Data-access contract for user entity persistence and query operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Returns true if any user already has the given email or phone number.
    /// </summary>
    Task<bool> ExistsAsync(string email, string phone);
    /// <summary>
    /// Retrieves a user by their email address, or null if not found.
    /// </summary>
    Task<User?> FindByEmailAsync(string email);
    /// <summary>
    /// Retrieves a user by their unique identifier, or null if not found.
    /// </summary>
    Task<User?> FindByIdAsync(Guid id);
    /// <summary>
    /// Returns all users ordered by registration date descending.
    /// </summary>
    Task<List<User>> GetAllAsync();
    /// <summary>
    /// Stages a new user entity for insertion into the data store.
    /// </summary>
    Task AddAsync(User user);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
