namespace AdminService.Domain.Entities;

/// <summary>Audit trail of admin actions.</summary>
public class AdminActivityLog
{
    /// <summary>
    /// Unique identifier of this activity log entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the admin user who performed the action.
    /// </summary>
    public Guid AdminUserId { get; set; }
    /// <summary>
    /// Name of the action performed (e.g., KYCApproved, KYCRejected, CampaignCreated).
    /// </summary>
    public string Action { get; set; } = string.Empty;      // KYCApproved | KYCRejected | CampaignCreated | etc.
    /// <summary>
    /// Type of the entity that was acted upon (e.g., KYCReview, Campaign, User).
    /// </summary>
    public string TargetType { get; set; } = string.Empty;  // KYCReview | Campaign | User
    /// <summary>
    /// Primary key of the target entity that was acted upon.
    /// </summary>
    public Guid TargetId { get; set; }
    /// <summary>
    /// Optional free-text details or notes associated with the action.
    /// </summary>
    public string? Details { get; set; }
    /// <summary>
    /// UTC timestamp when the action was recorded.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
