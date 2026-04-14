namespace RewardsService.Domain.Entities;

/// <summary>
/// Represents a user's rewards account, tracking balance, tier, and lifetime points.
/// </summary>
public class RewardsAccount
{
    /// <summary>
    /// Unique identifier of the rewards account.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the application user who owns this account.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Current redeemable points balance.
    /// </summary>
    public int PointsBalance { get; set; } = 0;
    /// <summary>
    /// Current membership tier (Bronze, Silver, Gold, Platinum).
    /// </summary>
    public string Tier { get; set; } = "Silver";        // Silver / Gold / Platinum
    /// <summary>
    /// Cumulative points ever earned, used to determine tier progression.
    /// </summary>
    public int LifetimePoints { get; set; } = 0;
    /// <summary>
    /// UTC timestamp when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the account was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// Collection of all points transactions associated with this account.
    /// </summary>
    public ICollection<RewardsTransaction> Transactions { get; set; } = new List<RewardsTransaction>();
    /// <summary>
    /// Collection of all redemptions made from this account.
    /// </summary>
    public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
