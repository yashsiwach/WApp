using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using MassTransit;
using SharedContracts.Events;

namespace AdminService.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles UserKYCSubmitted events and creates admin review queue entries.
/// </summary>
public class UserKYCSubmittedConsumer : IConsumer<UserKYCSubmitted>
{
    private readonly IKYCReviewRepository _reviews;
    private readonly ILogger<UserKYCSubmittedConsumer> _logger;

    /// <summary>
    /// Injects the KYC review repository and logger required for message processing.
    /// </summary>
    public UserKYCSubmittedConsumer(IKYCReviewRepository reviews, ILogger<UserKYCSubmittedConsumer> logger)
    {
        _reviews = reviews;
        _logger  = logger;
    }

    /// <summary>
    /// Processes an incoming UserKYCSubmitted event by creating a new pending review record if one does not already exist.
    /// </summary>
    public async Task Consume(ConsumeContext<UserKYCSubmitted> context)
    {
        var msg = context.Message;

        // Duplicate-check: skip processing if a review for this document already exists
        if (await _reviews.ExistsByDocumentIdAsync(msg.DocumentId)) return;

        // KycReview creation: project the event message into a new Pending review entity
        await _reviews.AddAsync(new KYCReview
        {
            DocumentId  = msg.DocumentId,
            UserId      = msg.UserId,
            DocType     = msg.DocType,
            FileUrl     = msg.FileUrl,
            Status      = "Pending",
            SubmittedAt = msg.OccurredAt
        });

        await _reviews.SaveAsync();
        _logger.LogInformation("KYC review queued: Document {DocId} for User {UserId}", msg.DocumentId, msg.UserId);
    }
}
