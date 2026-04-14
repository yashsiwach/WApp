namespace RewardsService.Domain.Entities;

/// <summary>Every point earn or deduct is recorded here (source of truth for points).</summary>
public class RewardsTransaction
{
    /// <summary>
    /// Unique identifier of the transaction record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Foreign key referencing the rewards account affected by this transaction.
    /// </summary>
    public Guid RewardsAccountId { get; set; }
    /// <summary>
    /// Transaction type (EARN, REDEEM, EXPIRE, ADJUST).
    /// </summary>
    public string Type { get; set; } = string.Empty;       // EARN | REDEEM | EXPIRE | ADJUST
    /// <summary>
    /// Points change applied to the account; positive for earn, negative for redeem or expire.
    /// </summary>
    public int PointsDelta { get; set; }                   // positive = earn, negative = redeem/expire
    /// <summary>
    /// Origin event category (TopUp, Transfer, Redemption).
    /// </summary>
    public string ReferenceType { get; set; } = string.Empty; // TopUp | Transfer | Redemption
    /// <summary>
    /// Optional identifier of the originating entity (e.g. a transaction or redemption ID).
    /// </summary>
    public Guid? ReferenceId { get; set; }
    /// <summary>
    /// Human-readable description of the reason for this transaction.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the transaction was recorded.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the owning rewards account.
    /// </summary>
    public RewardsAccount RewardsAccount { get; set; } = null!;
}
