using AdminService.Application.DTOs;
using SharedContracts.DTOs;

namespace AdminService.Application.Interfaces;

public interface IKYCManagementService
{
    Task<PaginatedResult<KYCReviewDto>> GetPendingAsync(int page, int size);
    Task<KYCReviewDto> GetByIdAsync(Guid reviewId);
    Task<KYCReviewDto> ApproveAsync(Guid reviewId, Guid adminId, KYCApproveRequest request);
    Task<KYCReviewDto> RejectAsync(Guid reviewId, Guid adminId, KYCRejectRequest request);
}
