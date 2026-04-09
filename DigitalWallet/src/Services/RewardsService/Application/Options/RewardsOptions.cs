namespace RewardsService.Application.Options;

public sealed class TierConfig
{
    public string Tier      { get; set; } = string.Empty;
    public int    MinPoints { get; set; }
}

public sealed class RewardsOptions
{
    public const string SectionName = "RewardsTiers";

    public List<TierConfig> Tiers { get; set; } =
    [
        new() { Tier = "Platinum", MinPoints = 10_000 },
        new() { Tier = "Gold",     MinPoints = 5_000  },
        new() { Tier = "Silver",   MinPoints = 1_000  },
        new() { Tier = "Bronze",   MinPoints = 0      },
    ];
}
