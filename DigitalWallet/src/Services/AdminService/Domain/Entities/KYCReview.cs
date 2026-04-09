namespace AdminService.Domain.Entities;

/// <summary>Local projection of a KYC submission for admin review queue.</summary>
public class KYCReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }        // from UserKYCSubmitted
    public Guid UserId { get; set; }
    public string DocType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";   // Pending | Approved | Rejected
    public string? ReviewNotes { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
