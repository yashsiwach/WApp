namespace RewardsService.Application.Options;

/// <summary>
/// Configuration entry that maps a tier name to its minimum lifetime points threshold.
/// </summary>
public sealed class TierConfig
{
    /// <summary>
    /// Tier name displayed to the user (Bronze, Silver, Gold, Platinum).
    /// </summary>
    public string Tier      { get; set; } = string.Empty;
    /// <summary>
    /// Minimum lifetime points required to reach this tier.
    /// </summary>
    public int    MinPoints { get; set; }
}

/// <summary>
/// Options bound from the RewardsTiers configuration section, defining tier thresholds.
/// </summary>
public sealed class RewardsOptions
{
    /// <summary>
    /// Configuration section name used for binding in appsettings.
    /// </summary>
    public const string SectionName = "RewardsTiers";

    /// <summary>
    /// Ordered list of tier configurations, highest threshold first.
    /// </summary>
    public List<TierConfig> Tiers { get; set; } =
    [
        new() { Tier = "Platinum", MinPoints = 10_000 },
        new() { Tier = "Gold",     MinPoints = 5_000  },
        new() { Tier = "Silver",   MinPoints = 1_000  },
        new() { Tier = "Bronze",   MinPoints = 0      },
    ];
}
