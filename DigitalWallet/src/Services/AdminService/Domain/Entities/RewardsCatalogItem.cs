namespace AdminService.Domain.Entities;

/// <summary>
/// Represents a redeemable item in the rewards catalog managed by admins.
/// </summary>
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
    /// Detailed description of the catalog item.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Number of reward points required to redeem this item.
    /// </summary>
    public int PointsCost { get; set; }
    /// <summary>
    /// Category grouping for the catalog item (e.g., Voucher, Merchandise).
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Whether the item is currently available for user redemption.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    /// <summary>
    /// Remaining redeemable stock; -1 indicates unlimited quantity.
    /// </summary>
    public int StockQuantity { get; set; } = -1;
    /// <summary>
    /// UTC timestamp when the catalog item was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the catalog item was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
