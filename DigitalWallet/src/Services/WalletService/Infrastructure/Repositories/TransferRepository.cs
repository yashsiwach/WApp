using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITransferRepository for persisting and deduplicating transfer requests.
/// </summary>
public class TransferRepository : ITransferRepository
{
    private readonly WalletDbContext _db;
    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public TransferRepository(WalletDbContext db) => _db = db;

    /// <summary>
    /// Returns the transfer request matching the given idempotency key, or null if not found.
    /// </summary>
    public Task<TransferRequest?> FindByIdempotencyKeyAsync(string key) =>
        _db.TransferRequests.FirstOrDefaultAsync(t => t.IdempotencyKey == key);

    /// <summary>
    /// Stages a new transfer request for insertion on the next SaveAsync call.
    /// </summary>
    public async Task AddAsync(TransferRequest request) =>
        await _db.TransferRequests.AddAsync(request);
}
