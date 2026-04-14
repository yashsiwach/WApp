namespace AuthService.Domain.Entities;

/// <summary>
/// Represents a KYC identity document submitted by a user for review.
/// </summary>
public class KYCDocument
{
    /// <summary>
    /// Unique identifier for the KYC document record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the user who submitted this document.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Category of the identity document (e.g., Aadhaar, PAN, Passport).
    /// </summary>
    public string DocType { get; set; } = string.Empty; // Aadhaar, PAN, Passport
    /// <summary>
    /// URL pointing to the uploaded document file.
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;
    /// <summary>
    /// Current review status of the document: Pending, Approved, or Rejected.
    /// </summary>
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    /// <summary>
    /// Optional reviewer notes recorded during the approval or rejection decision.
    /// </summary>
    public string? ReviewNotes { get; set; }
    /// <summary>
    /// UTC timestamp when the document was originally submitted.
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the record was created in the database.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the most recent update to this record.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// Navigation property to the owning user entity.
    /// </summary>
    public User User { get; set; } = null!;
}
