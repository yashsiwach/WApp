namespace RewardsService.Domain.Entities;

/// <summary>Defines how many points are earned per transaction type.</summary>
public class EarnRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TriggerType { get; set; } = string.Empty;  // TopUp | Transfer
    public decimal PointsPerRupee { get; set; } = 1m;        // e.g. 1 point per ₹100 = 0.01
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
