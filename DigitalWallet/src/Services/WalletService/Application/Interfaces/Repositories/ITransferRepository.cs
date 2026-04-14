using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories;

/// <summary>
/// Repository for persisting and deduplicating transfer request records.
/// </summary>
public interface ITransferRepository
{
    /// <summary>
    /// Returns the transfer request matching the given idempotency key, or null if not found.
    /// </summary>
    Task<TransferRequest?> FindByIdempotencyKeyAsync(string key);
    /// <summary>
    /// Stages a new transfer request for insertion on the next SaveAsync call.
    /// </summary>
    Task AddAsync(TransferRequest request);
}
