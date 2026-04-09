using AdminService.Application.DTOs;
using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;

namespace AdminService.Infrastructure.Repositories;

public class KYCReviewRepository : IKYCReviewRepository
{
    private readonly AdminDbContext _db;
    public KYCReviewRepository(AdminDbContext db) => _db = db;

    public Task<bool> ExistsByDocumentIdAsync(Guid documentId) =>
        _db.KYCReviews.AnyAsync(k => k.DocumentId == documentId);

    public async Task AddAsync(KYCReview review) =>
        await _db.KYCReviews.AddAsync(review);

    public Task<KYCReview?> FindByIdAsync(Guid id) =>
        _db.KYCReviews.FindAsync(id).AsTask();

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

    public Task SaveAsync() => _db.SaveChangesAsync();
}
