using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository for user entity data access.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;
    /// <summary>
    /// Initializes the repository with the AuthService database context.
    /// </summary>
    public UserRepository(AuthDbContext db) => _db = db;

    /// <summary>
    /// Returns true if any existing user has the given email address or phone number.
    /// </summary>
    public Task<bool> ExistsAsync(string email, string phone) => _db.Users.AnyAsync(u => u.Email == email || u.Phone == phone);

    /// <summary>
    /// Retrieves a user by their email address, or null if no match is found.
    /// </summary>
    public Task<User?> FindByEmailAsync(string email) =>_db.Users.FirstOrDefaultAsync(u => u.Email == email);

    /// <summary>
    /// Retrieves a user by their unique identifier, or null if not found.
    /// </summary>
    public Task<User?> FindByIdAsync(Guid id) =>_db.Users.FindAsync(id).AsTask();

    /// <summary>
    /// Returns all users without change tracking, sorted by registration date descending.
    /// </summary>
    public Task<List<User>> GetAllAsync() =>
        _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

    /// <summary>
    /// Stages a new user entity for insertion into the database.
    /// </summary>
    public async Task AddAsync(User user) =>await _db.Users.AddAsync(user);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
