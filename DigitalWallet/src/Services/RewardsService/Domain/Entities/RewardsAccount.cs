namespace RewardsService.Domain.Entities;

public class RewardsAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int PointsBalance { get; set; } = 0;
    public string Tier { get; set; } = "Silver";        // Silver / Gold / Platinum
    public int LifetimePoints { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<RewardsTransaction> Transactions { get; set; } = new List<RewardsTransaction>();
    public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
