using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly WalletDbContext _db;
    public TransferRepository(WalletDbContext db) => _db = db;

    public Task<TransferRequest?> FindByIdempotencyKeyAsync(string key) =>
        _db.TransferRequests.FirstOrDefaultAsync(t => t.IdempotencyKey == key);

    public async Task AddAsync(TransferRequest request) =>
        await _db.TransferRequests.AddAsync(request);
}
