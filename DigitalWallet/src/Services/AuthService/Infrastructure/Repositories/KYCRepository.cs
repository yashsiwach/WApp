using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IKYCRepository for KYC document data access.
/// </summary>
public class KYCRepository : IKYCRepository
{
    private readonly AuthDbContext _db;
    /// <summary>
    /// Initializes the repository with the AuthService database context.
    /// </summary>
    public KYCRepository(AuthDbContext db) => _db = db;

    /// <summary>
    /// Returns true if the user has a KYC document of the given type currently awaiting review.
    /// </summary>
    public Task<bool> HasPendingAsync(Guid userId, string docType) =>_db.KYCDocuments.AnyAsync(k => k.UserId == userId && k.DocType == docType && k.Status == "Pending");

    /// <summary>
    /// Looks up a user entity by their unique identifier.
    /// </summary>
    public Task<User?> FindUserByIdAsync(Guid userId) => _db.Users.FindAsync(userId).AsTask();

    /// <summary>
    /// Stages a new KYC document entity for insertion into the database.
    /// </summary>
    public async Task AddAsync(KYCDocument doc) => await _db.KYCDocuments.AddAsync(doc);

    /// <summary>
    /// Returns all KYC documents for the user sorted by submission date descending.
    /// </summary>
    public Task<List<KYCDocument>> GetByUserIdAsync(Guid userId) => _db.KYCDocuments
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.SubmittedAt)
            .ToListAsync();

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
