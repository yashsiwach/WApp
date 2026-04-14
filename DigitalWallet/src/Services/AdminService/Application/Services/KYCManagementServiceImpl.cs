using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Application.Interfaces.Repositories;
using AdminService.Application.Mappers;
using AdminService.Domain.Entities;
using MassTransit;
using SharedContracts.DTOs;
using SharedContracts.Events;

namespace AdminService.Application.Services;

public class KYCManagementServiceImpl : IKYCManagementService
{
    private readonly IKYCReviewRepository _reviews;
    private readonly IActivityLogRepository _logs;
    private readonly IPublishEndpoint _bus;
    private readonly ILogger<KYCManagementServiceImpl> _logger;

    /// <summary>Initializes KYC review dependencies for data access, auditing, event publishing, and logging.</summary>
    public KYCManagementServiceImpl(
        IKYCReviewRepository reviews,
        IActivityLogRepository logs,
        IPublishEndpoint bus,
        ILogger<KYCManagementServiceImpl> logger)
    {
        _reviews = reviews;
        _logs    = logs;
        _bus     = bus;
        _logger  = logger;
    }

    /// <summary>Returns paginated pending KYC reviews for admin processing.</summary>
    public Task<PaginatedResult<KYCReviewDto>> GetPendingAsync(int page, int size) =>_reviews.GetPendingPagedAsync(page, size);

    /// <summary>Fetches a KYC review by id and maps it to DTO or throws when missing.</summary>
    public async Task<KYCReviewDto> GetByIdAsync(Guid reviewId)
    {
        var review = await _reviews.FindByIdAsync(reviewId)?? throw new KeyNotFoundException("KYC review not found.");
        return AdminMapper.ToDto(review);
    }

    /// <summary>Approves a pending KYC review, records admin activity, publishes approval event, and returns updated DTO.</summary>
    public async Task<KYCReviewDto> ApproveAsync(Guid reviewId, Guid adminId, KYCApproveRequest request)
    {
        var review = await _reviews.FindByIdAsync(reviewId)?? throw new KeyNotFoundException("KYC review not found.");

        if (review.Status != "Pending")throw new InvalidOperationException($"Cannot approve a review with status '{review.Status}'.");

        review.Status      = "Approved";
        review.ReviewNotes = request.Notes;
        review.ReviewedBy  = adminId;
        review.ReviewedAt  = DateTime.UtcNow;

        await _logs.AddAsync(new AdminActivityLog
        {
            AdminUserId = adminId,
            Action      = "KYCApproved",
            TargetType  = "KYCReview",
            TargetId    = review.Id,
            Details     = request.Notes
        });

        await _reviews.SaveAsync();

        await _bus.Publish(new KYCApproved
        {
            UserId     = review.UserId,
            DocumentId = review.DocumentId,
            ReviewedBy = adminId,
            Notes      = request.Notes,
            OccurredAt = DateTime.UtcNow
        });

        _logger.LogInformation("KYC approved: Document {DocId} for User {UserId} by Admin {AdminId}",review.DocumentId, review.UserId, adminId);

        return AdminMapper.ToDto(review);
    }

    /// <summary>Rejects a pending KYC review, records admin activity, publishes rejection event, and returns updated DTO.</summary>
    public async Task<KYCReviewDto> RejectAsync(Guid reviewId, Guid adminId, KYCRejectRequest request)
    {
        var review = await _reviews.FindByIdAsync(reviewId)?? throw new KeyNotFoundException("KYC review not found.");

        if (review.Status != "Pending")throw new InvalidOperationException($"Cannot reject a review with status '{review.Status}'.");

        review.Status      = "Rejected";
        review.ReviewNotes = request.Reason;
        review.ReviewedBy  = adminId;
        review.ReviewedAt  = DateTime.UtcNow;

        await _logs.AddAsync(new AdminActivityLog
        {
            AdminUserId = adminId,
            Action      = "KYCRejected",
            TargetType  = "KYCReview",
            TargetId    = review.Id,
            Details     = request.Reason
        });

        await _reviews.SaveAsync();

        await _bus.Publish(new KYCRejected
        {
            UserId     = review.UserId,
            DocumentId = review.DocumentId,
            ReviewedBy = adminId,
            Reason     = request.Reason,
            OccurredAt = DateTime.UtcNow
        });

        _logger.LogInformation("KYC rejected: Document {DocId} for User {UserId} by Admin {AdminId}",review.DocumentId, review.UserId, adminId);

        return AdminMapper.ToDto(review);
    }

}
