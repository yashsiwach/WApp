namespace AdminService.Domain.Entities;

/// <summary>Local projection of a KYC submission for admin review queue.</summary>
public class KYCReview
{
    /// <summary>
    /// Unique identifier of this KYC review record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the originating KYC document from the UserKYCSubmitted event.
    /// </summary>
    public Guid DocumentId { get; set; }        // from UserKYCSubmitted
    /// <summary>
    /// Identifier of the user who submitted the KYC document.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Type of identity document submitted (e.g., Passport, NationalId).
    /// </summary>
    public string DocType { get; set; } = string.Empty;
    /// <summary>
    /// URL of the uploaded document file.
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;
    /// <summary>
    /// Current review status: Pending, Approved, or Rejected.
    /// </summary>
    public string Status { get; set; } = "Pending";   // Pending | Approved | Rejected
    /// <summary>
    /// Admin notes or rejection reason recorded during review.
    /// </summary>
    public string? ReviewNotes { get; set; }
    /// <summary>
    /// Identifier of the admin user who reviewed this document.
    /// </summary>
    public Guid? ReviewedBy { get; set; }
    /// <summary>
    /// UTC timestamp when the user originally submitted the document.
    /// </summary>
    public DateTime SubmittedAt { get; set; }
    /// <summary>
    /// UTC timestamp when an admin completed the review, or null if still pending.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    /// <summary>
    /// UTC timestamp when this review record was created in the admin service.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
