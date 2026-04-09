namespace AdminService.Domain.Entities;

/// <summary>Audit trail of admin actions.</summary>
public class AdminActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty;      // KYCApproved | KYCRejected | CampaignCreated | etc.
    public string TargetType { get; set; } = string.Empty;  // KYCReview | Campaign | User
    public Guid TargetId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
