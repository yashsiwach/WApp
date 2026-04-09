using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Repositories;

public class TopUpRepository : ITopUpRepository
{
    private readonly WalletDbContext _db;
    public TopUpRepository(WalletDbContext db) => _db = db;

    public Task<TopUpRequest?> FindByIdempotencyKeyAsync(string key) =>
        _db.TopUpRequests.FirstOrDefaultAsync(t => t.IdempotencyKey == key);

    public async Task AddAsync(TopUpRequest request) =>
        await _db.TopUpRequests.AddAsync(request);
}
