namespace RewardsService.Domain.Entities;

/// <summary>Defines how many points are earned per transaction type.</summary>
public class EarnRule
{
    /// <summary>
    /// Unique identifier of the earn rule.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Human-readable name for this earn rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Event type that activates this rule (TopUp or Transfer).
    /// </summary>
    public string TriggerType { get; set; } = string.Empty;  // TopUp | Transfer
    /// <summary>
    /// Points awarded per unit of currency (e.g. 0.01 = 1 point per ₹100).
    /// </summary>
    public decimal PointsPerRupee { get; set; } = 1m;        // e.g. 1 point per ₹100 = 0.01
    /// <summary>
    /// Whether this rule is currently active and eligible to award points.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// UTC timestamp when the earn rule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
