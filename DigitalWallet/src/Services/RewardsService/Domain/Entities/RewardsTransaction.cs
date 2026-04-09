namespace RewardsService.Domain.Entities;

/// <summary>Every point earn or deduct is recorded here (source of truth for points).</summary>
public class RewardsTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RewardsAccountId { get; set; }
    public string Type { get; set; } = string.Empty;       // EARN | REDEEM | EXPIRE | ADJUST
    public int PointsDelta { get; set; }                   // positive = earn, negative = redeem/expire
    public string ReferenceType { get; set; } = string.Empty; // TopUp | Transfer | Redemption
    public Guid? ReferenceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RewardsAccount RewardsAccount { get; set; } = null!;
}
