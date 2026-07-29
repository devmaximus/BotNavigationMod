using BepInEx.Configuration;

namespace BotNavigationMod.Config;

public sealed class PatrolPointOffsetConfig
{
    public ConfigEntry<bool> EnabledEntry { get; }
    public ConfigEntry<float> MinOffsetEntry { get; }
    public ConfigEntry<float> MaxOffsetEntry { get; }
    public ConfigEntry<float> SampleRadiusEntry { get; }
    public ConfigEntry<float> MaxYDeltaEntry { get; }
    public ConfigEntry<int> GroupSizeThresholdEntry { get; }

    public bool IsEnabled => EnabledEntry.Value;
    public float MinOffset => MinOffsetEntry.Value;
    public float MaxOffset => MaxOffsetEntry.Value;
    public float SampleRadius => SampleRadiusEntry.Value;
    public float MaxYDelta => MaxYDeltaEntry.Value;
    public int GroupSizeThreshold => GroupSizeThresholdEntry.Value;

    public PatrolPointOffsetConfig(ConfigFile cfg)
    {
        const string section = "Strategy.PatrolPointOffset";

        EnabledEntry = cfg.Bind(section, "Enabled", true, "Enable patrol point offset fan-out");
        MinOffsetEntry = cfg.Bind(section, "MinOffset", 3f,
            new ConfigDescription("Minimum perpendicular offset (meters)", new AcceptableValueRange<float>(1f, 10f)));
        MaxOffsetEntry = cfg.Bind(section, "MaxOffset", 8f,
            new ConfigDescription("Maximum perpendicular offset (meters)", new AcceptableValueRange<float>(3f, 20f)));
        SampleRadiusEntry = cfg.Bind(section, "SampleRadius", 2f,
            new ConfigDescription("NavMesh sample search radius", new AcceptableValueRange<float>(0.5f, 5f)));
        MaxYDeltaEntry = cfg.Bind(section, "MaxYDelta", 2f,
            new ConfigDescription("Reject NavMesh samples when vertical delta exceeds this", new AcceptableValueRange<float>(0.5f, 10f)));
        GroupSizeThresholdEntry = cfg.Bind(section, "GroupSizeThreshold", 2,
            new ConfigDescription("Minimum group size to activate", new AcceptableValueRange<int>(1, 20)));
    }
}
