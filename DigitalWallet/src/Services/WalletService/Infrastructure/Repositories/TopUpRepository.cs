using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITopUpRepository for persisting and deduplicating top-up requests.
/// </summary>
public class TopUpRepository : ITopUpRepository
{
    private readonly WalletDbContext _db;
    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public TopUpRepository(WalletDbContext db) => _db = db;

    /// <summary>
    /// Returns the top-up request matching the given idempotency key, or null if not found.
    /// </summary>
    public Task<TopUpRequest?> FindByIdempotencyKeyAsync(string key) =>
        _db.TopUpRequests.FirstOrDefaultAsync(t => t.IdempotencyKey == key);

    /// <summary>
    /// Stages a new top-up request for insertion on the next SaveAsync call.
    /// </summary>
    public async Task AddAsync(TopUpRequest request) =>
        await _db.TopUpRequests.AddAsync(request);
}
