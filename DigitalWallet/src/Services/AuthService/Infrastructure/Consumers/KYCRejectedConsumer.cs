using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Events;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Consumers;

/// <summary>
/// Keeps AuthService KYC document status in sync when admin rejects KYC.
/// </summary>
public class KYCRejectedConsumer : IConsumer<KYCRejected>
{
    private readonly AuthDbContext _db;
    private readonly ILogger<KYCRejectedConsumer> _logger;

    /// <summary>
    /// Initializes the consumer with database and logging dependencies.
    /// </summary>
    public KYCRejectedConsumer(AuthDbContext db, ILogger<KYCRejectedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Processes a KYCRejected event by updating the corresponding document status to Rejected.
    /// </summary>
    public async Task Consume(ConsumeContext<KYCRejected> context)
    {
        var msg = context.Message;

        var doc = await _db.KYCDocuments
            .FirstOrDefaultAsync(k => k.Id == msg.DocumentId && k.UserId == msg.UserId);

        if (doc == null)
        {
            _logger.LogWarning(
                "KYC document {DocumentId} not found in AuthService for rejected user {UserId}",
                msg.DocumentId,
                msg.UserId);
            return;
        }

        if (doc.Status == "Rejected")
        {
            var incomingReason = string.IsNullOrWhiteSpace(msg.Reason) ? null : msg.Reason;
            if (doc.ReviewNotes != incomingReason)
            {
                doc.ReviewNotes = incomingReason;
                doc.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated rejection reason for already rejected KYC document {DocumentId} for user {UserId}",
                    msg.DocumentId,
                    msg.UserId);
            }
            else
            {
                _logger.LogInformation(
                    "KYC document {DocumentId} for user {UserId} was already rejected",
                    msg.DocumentId,
                    msg.UserId);
            }

            return;
        }

        if (doc.Status != "Pending")
        {
            _logger.LogWarning(
                "KYC document {DocumentId} for user {UserId} is in status {Status} and cannot be rejected",
                msg.DocumentId,
                msg.UserId,
                doc.Status);
            return;
        }

        doc.Status = "Rejected";
        doc.ReviewNotes = string.IsNullOrWhiteSpace(msg.Reason) ? null : msg.Reason;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "AuthService KYC status updated to Rejected for user {UserId}, document {DocumentId}",
            msg.UserId,
            doc.Id);
    }
}
