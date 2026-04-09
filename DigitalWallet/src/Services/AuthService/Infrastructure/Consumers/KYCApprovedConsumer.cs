using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Events;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Consumers;

/// <summary>
/// Keeps AuthService KYC document status in sync when admin approves KYC.
/// </summary>
public class KYCApprovedConsumer : IConsumer<KYCApproved>
{
    private readonly AuthDbContext _db;
    private readonly ILogger<KYCApprovedConsumer> _logger;

    public KYCApprovedConsumer(AuthDbContext db, ILogger<KYCApprovedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<KYCApproved> context)
    {
        var msg = context.Message;

        var doc = await _db.KYCDocuments
            .FirstOrDefaultAsync(k => k.Id == msg.DocumentId && k.UserId == msg.UserId);

        if (doc == null)
        {
            _logger.LogWarning(
                "KYC document {DocumentId} not found in AuthService for approved user {UserId}",
                msg.DocumentId,
                msg.UserId);
            return;
        }

        if (doc.Status == "Approved")
        {
            var incomingNotes = string.IsNullOrWhiteSpace(msg.Notes) ? null : msg.Notes;
            if (doc.ReviewNotes != incomingNotes)
            {
                doc.ReviewNotes = incomingNotes;
                doc.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated approval notes for already approved KYC document {DocumentId} for user {UserId}",
                    msg.DocumentId,
                    msg.UserId);
            }
            else
            {
                _logger.LogInformation(
                    "KYC document {DocumentId} for user {UserId} was already approved",
                    msg.DocumentId,
                    msg.UserId);
            }

            return;
        }

        if (doc.Status != "Pending")
        {
            _logger.LogWarning(
                "KYC document {DocumentId} for user {UserId} is in status {Status} and cannot be approved",
                msg.DocumentId,
                msg.UserId,
                doc.Status);
            return;
        }

        doc.Status = "Approved";
        doc.ReviewNotes = string.IsNullOrWhiteSpace(msg.Notes) ? null : msg.Notes;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "AuthService KYC status updated to Approved for user {UserId}, document {DocumentId}",
            msg.UserId,
            doc.Id);
    }
}
