namespace RewardsService.Domain.Entities;

/// <summary>Item in the rewards catalog that users can redeem points for.</summary>
public class RewardsCatalogItem
{
    /// <summary>
    /// Unique identifier of the catalog item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Display name of the catalog item.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Detailed description of what the item offers.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Number of points required to redeem this item.
    /// </summary>
    public int PointsCost { get; set; }
    /// <summary>
    /// Item category (Voucher, Cashback, Gift).
    /// </summary>
    public string Category { get; set; } = string.Empty;    // Voucher | Cashback | Gift
    /// <summary>
    /// Whether the item is currently offered for redemption.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    /// <summary>
    /// Remaining inventory; -1 indicates unlimited stock.
    /// </summary>
    public int StockQuantity { get; set; } = -1;            // -1 = unlimited
    /// <summary>
    /// UTC timestamp when the item was added to the catalog.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the item was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of redemptions made for this catalog item.
    /// </summary>
    public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
