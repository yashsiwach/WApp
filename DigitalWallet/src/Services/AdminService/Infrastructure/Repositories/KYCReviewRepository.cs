using AdminService.Application.DTOs;
using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;

namespace AdminService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IKYCReviewRepository providing data access for KYC review records.
/// </summary>
public class KYCReviewRepository : IKYCReviewRepository
{
    private readonly AdminDbContext _db;
    /// <summary>
    /// Injects the AdminDbContext used for all KYC review data operations.
    /// </summary>
    public KYCReviewRepository(AdminDbContext db) => _db = db;

    /// <summary>
    /// Returns true if a KYC review record already exists for the specified document ID.
    /// </summary>
    public Task<bool> ExistsByDocumentIdAsync(Guid documentId) => _db.KYCReviews.AnyAsync(k => k.DocumentId == documentId);

    /// <summary>
    /// Stages a new KYC review entity into the EF change tracker for later persistence.
    /// </summary>
    public async Task AddAsync(KYCReview review) =>await _db.KYCReviews.AddAsync(review);

    /// <summary>
    /// Retrieves a KYC review by its primary key, or returns null when not found.
    /// </summary>
    public Task<KYCReview?> FindByIdAsync(Guid id) =>_db.KYCReviews.FindAsync(id).AsTask();

    /// <summary>
    /// Returns a paginated, chronologically ordered list of KYC reviews with Pending status.
    /// </summary>
    public async Task<PaginatedResult<KYCReviewDto>> GetPendingPagedAsync(int page, int size)
    {
        var query = _db.KYCReviews.Where(k => k.Status == "Pending");
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(k => k.SubmittedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(k => new KYCReviewDto
            {
                Id          = k.Id,
                DocumentId  = k.DocumentId,
                UserId      = k.UserId,
                DocType     = k.DocType,
                FileUrl     = k.FileUrl,
                Status      = k.Status,
                ReviewNotes = k.ReviewNotes,
                SubmittedAt = k.SubmittedAt,
                ReviewedAt  = k.ReviewedAt
            })
            .ToListAsync();

        return new PaginatedResult<KYCReviewDto> { Items = items, Page = page, PageSize = size, TotalCount = total };
    }

    /// <summary>
    /// Flushes all pending EF change tracker changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
