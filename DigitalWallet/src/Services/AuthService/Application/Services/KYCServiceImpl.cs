using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Repositories;
using AuthService.Application.Mappers;
using AuthService.Domain.Entities;
using MassTransit;
using SharedContracts.Events;

namespace AuthService.Application.Services;

public class KYCServiceImpl : IKYCService
{
    private readonly IKYCRepository _kyc;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<KYCServiceImpl> _logger;

    /// <summary>Initializes KYC persistence, event publishing, and logging dependencies.</summary>
    public KYCServiceImpl(IKYCRepository kyc, IPublishEndpoint publishEndpoint, ILogger<KYCServiceImpl> logger)
    {
        _kyc = kyc;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>Validates and stores a new KYC document submission, then publishes a KYC submitted event.</summary>
    public async Task<KYCStatusResponse> SubmitAsync(Guid userId, KYCSubmitRequest request)
    {
        var user = await _kyc.FindUserByIdAsync(userId)?? throw new InvalidOperationException("User not found.");

        var hasPending = await _kyc.HasPendingAsync(userId, request.DocType);
        if (hasPending)
            throw new InvalidOperationException($"A {request.DocType} document is already pending review.");

        var doc = new KYCDocument
        {
            UserId = userId,
            DocType = request.DocType,
            FileUrl = request.FileUrl,
            Status = "Pending",
            SubmittedAt = DateTime.UtcNow
        };

        await _kyc.AddAsync(doc);
        await _kyc.SaveAsync();

        _logger.LogInformation("KYC submitted: {DocId} for user {UserId}", doc.Id, userId);

        await _publishEndpoint.Publish(new UserKYCSubmitted
        {
            UserId = userId,
            DocumentId = doc.Id,
            DocType = doc.DocType,
            FileUrl = doc.FileUrl,
            OccurredAt = DateTime.UtcNow
        });

        return AuthMapper.ToDto(doc);
    }

    /// <summary>Retrieves all KYC documents for a user and returns their mapped status responses.</summary>
    public async Task<List<KYCStatusResponse>> GetStatusAsync(Guid userId)
    {
        var docs = await _kyc.GetByUserIdAsync(userId);
        return docs.Select(AuthMapper.ToDto).ToList();
    }
}
