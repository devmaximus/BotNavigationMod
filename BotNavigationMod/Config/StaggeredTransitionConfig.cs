using BepInEx.Configuration;

namespace BotNavigationMod.Config;

public sealed class StaggeredTransitionConfig
{
    public ConfigEntry<bool> EnabledEntry { get; }
    public ConfigEntry<float> BaseDelayEntry { get; }
    public ConfigEntry<float> PerMemberDelayEntry { get; }
    public ConfigEntry<float> MaxJitterEntry { get; }
    public ConfigEntry<float> MaxTotalDelayEntry { get; }

    public bool IsEnabled => EnabledEntry.Value;
    public float BaseDelay => BaseDelayEntry.Value;
    public float PerMemberDelay => PerMemberDelayEntry.Value;
    public float MaxJitter => MaxJitterEntry.Value;
    public float MaxTotalDelay => MaxTotalDelayEntry.Value;

    public StaggeredTransitionConfig(ConfigFile cfg)
    {
        const string section = "Strategy.StaggeredTransition";

        EnabledEntry = cfg.Bind(section, "Enabled", true, "Enable staggered patrol departures");
        BaseDelayEntry = cfg.Bind(section, "BaseDelay", 2f,
            new ConfigDescription("Minimum delay before departure (seconds)", new AcceptableValueRange<float>(0f, 10f)));
        PerMemberDelayEntry = cfg.Bind(section, "PerMemberDelay", 1.5f,
            new ConfigDescription("Additional delay per member index", new AcceptableValueRange<float>(0.5f, 5f)));
        MaxJitterEntry = cfg.Bind(section, "MaxJitter", 3f,
            new ConfigDescription("Random jitter range (seconds)", new AcceptableValueRange<float>(0f, 10f)));
        MaxTotalDelayEntry = cfg.Bind(section, "MaxTotalDelay", 15f,
            new ConfigDescription("Hard cap on computed delay", new AcceptableValueRange<float>(5f, 60f)));
    }
}
