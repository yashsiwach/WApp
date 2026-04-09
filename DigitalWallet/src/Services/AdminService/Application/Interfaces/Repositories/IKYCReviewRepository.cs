using AdminService.Application.DTOs;
using AdminService.Domain.Entities;
using SharedContracts.DTOs;

namespace AdminService.Application.Interfaces.Repositories;

public interface IKYCReviewRepository
{
    Task<bool> ExistsByDocumentIdAsync(Guid documentId);
    Task AddAsync(KYCReview review);
    Task<KYCReview?> FindByIdAsync(Guid id);
    Task<PaginatedResult<KYCReviewDto>> GetPendingPagedAsync(int page, int size);
    Task SaveAsync();
}
