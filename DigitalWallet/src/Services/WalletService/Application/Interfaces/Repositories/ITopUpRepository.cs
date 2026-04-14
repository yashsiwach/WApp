using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>
/// Repository for persisting and deduplicating top-up request records.
/// </summary>
public interface ITopUpRepository
{
    /// <summary>
    /// Returns the top-up request matching the given idempotency key, or null if not found.
    /// </summary>
    Task<TopUpRequest?> FindByIdempotencyKeyAsync(string key);
    /// <summary>
    /// Stages a new top-up request for insertion on the next SaveAsync call.
    /// </summary>
    Task AddAsync(TopUpRequest request);
}
