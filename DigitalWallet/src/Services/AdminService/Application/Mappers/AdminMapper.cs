using AdminService.Application.DTOs;
using AdminService.Domain.Entities;

namespace AdminService.Application.Mappers;

/// <summary>Single responsibility: maps AdminService domain objects to DTOs.</summary>
public static class AdminMapper
{
    public static KYCReviewDto ToDto(KYCReview k) => new()
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
    };

}
