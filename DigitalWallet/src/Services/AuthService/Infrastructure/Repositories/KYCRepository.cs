using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class KYCRepository : IKYCRepository
{
    private readonly AuthDbContext _db;
    public KYCRepository(AuthDbContext db) => _db = db;

    public Task<bool> HasPendingAsync(Guid userId, string docType) =>_db.KYCDocuments.AnyAsync(k => k.UserId == userId && k.DocType == docType && k.Status == "Pending");

    public Task<User?> FindUserByIdAsync(Guid userId) => _db.Users.FindAsync(userId).AsTask();

    public async Task AddAsync(KYCDocument doc) => await _db.KYCDocuments.AddAsync(doc);

    public Task<List<KYCDocument>> GetByUserIdAsync(Guid userId) => _db.KYCDocuments
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.SubmittedAt)
            .ToListAsync();

    public Task SaveAsync() => _db.SaveChangesAsync();
}
