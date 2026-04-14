using AdminService.Application.DTOs;
using AdminService.Domain.Entities;
using SharedContracts.DTOs;

namespace AdminService.Application.Interfaces.Repositories;

/// <summary>
/// Contract for data access operations on KYC review records.
/// </summary>
public interface IKYCReviewRepository
{
    /// <summary>
    /// Returns true when a KYC review already exists for the specified document ID.
    /// </summary>
    Task<bool> ExistsByDocumentIdAsync(Guid documentId);
    /// <summary>
    /// Stages a new KYC review entity for persistence.
    /// </summary>
    Task AddAsync(KYCReview review);
    /// <summary>
    /// Finds and returns a KYC review by its primary key, or null if not found.
    /// </summary>
    Task<KYCReview?> FindByIdAsync(Guid id);
    /// <summary>
    /// Returns a paginated list of KYC reviews that are currently in Pending status.
    /// </summary>
    Task<PaginatedResult<KYCReviewDto>> GetPendingPagedAsync(int page, int size);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
