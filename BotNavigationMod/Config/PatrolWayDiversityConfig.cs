using BepInEx.Configuration;

namespace BotNavigationMod.Config;

public sealed class PatrolWayDiversityConfig
{
    public ConfigEntry<bool> EnabledEntry { get; }
    public ConfigEntry<int> MaxBotsPerWayEntry { get; }
    public ConfigEntry<float> RebalanceIntervalEntry { get; }
    public ConfigEntry<bool> PreferNearbyWaysEntry { get; }
    public ConfigEntry<float> DistanceWeightEntry { get; }

    public bool IsEnabled => EnabledEntry.Value;
    public int MaxBotsPerWay => MaxBotsPerWayEntry.Value;
    public float RebalanceInterval => RebalanceIntervalEntry.Value;
    public bool PreferNearbyWays => PreferNearbyWaysEntry.Value;
    public float DistanceWeight => DistanceWeightEntry.Value;

    public PatrolWayDiversityConfig(ConfigFile cfg)
    {
        const string section = "Strategy.PatrolWayDiversity";

        EnabledEntry = cfg.Bind(section, "Enabled", true, "Enable patrol way diversity");
        MaxBotsPerWayEntry = cfg.Bind(section, "MaxBotsPerWay", 3,
            new ConfigDescription("Max bots on same way before rebalancing", new AcceptableValueRange<int>(1, 10)));
        RebalanceIntervalEntry = cfg.Bind(section, "RebalanceInterval", 120f,
            new ConfigDescription("Seconds between rebalance checks", new AcceptableValueRange<float>(30f, 600f)));
        PreferNearbyWaysEntry = cfg.Bind(section, "PreferNearbyWays", true,
            "Weight closer ways higher when breaking ties");
        DistanceWeightEntry = cfg.Bind(section, "DistanceWeight", 0.5f,
            new ConfigDescription("Distance vs load-balance weight", new AcceptableValueRange<float>(0f, 1f)));
    }
}
