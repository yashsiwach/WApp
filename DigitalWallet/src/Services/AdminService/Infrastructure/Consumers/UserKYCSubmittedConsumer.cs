using AdminService.Application.Interfaces.Repositories;
using AdminService.Domain.Entities;
using MassTransit;
using SharedContracts.Events;

namespace AdminService.Infrastructure.Consumers;

public class UserKYCSubmittedConsumer : IConsumer<UserKYCSubmitted>
{
    private readonly IKYCReviewRepository _reviews;
    private readonly ILogger<UserKYCSubmittedConsumer> _logger;

    public UserKYCSubmittedConsumer(IKYCReviewRepository reviews, ILogger<UserKYCSubmittedConsumer> logger)
    {
        _reviews = reviews;
        _logger  = logger;
    }

    public async Task Consume(ConsumeContext<UserKYCSubmitted> context)
    {
        var msg = context.Message;

        if (await _reviews.ExistsByDocumentIdAsync(msg.DocumentId)) return;

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
