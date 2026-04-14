using AdminService.Application.DTOs;
using SharedContracts.DTOs;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Contract for admin operations on KYC document review submissions.
/// </summary>
public interface IKYCManagementService
{
    /// <summary>
    /// Retrieves a paginated list of KYC reviews with Pending status.
    /// </summary>
    Task<PaginatedResult<KYCReviewDto>> GetPendingAsync(int page, int size);
    /// <summary>
    /// Fetches a single KYC review by its unique identifier.
    /// </summary>
    Task<KYCReviewDto> GetByIdAsync(Guid reviewId);
    /// <summary>
    /// Approves a pending KYC review on behalf of the given admin and publishes a KYCApproved event.
    /// </summary>
    Task<KYCReviewDto> ApproveAsync(Guid reviewId, Guid adminId, KYCApproveRequest request);
    /// <summary>
    /// Rejects a pending KYC review on behalf of the given admin and publishes a KYCRejected event.
    /// </summary>
    Task<KYCReviewDto> RejectAsync(Guid reviewId, Guid adminId, KYCRejectRequest request);
}
