using BepInEx.Configuration;

namespace BotNavigationMod.Config;

public sealed class NavigationConfig
{
    public ConfigEntry<bool> GlobalEnabledEntry { get; }

    public PatrolPointOffsetConfig PatrolPointOffset { get; }
    public PatrolWayDiversityConfig PatrolWayDiversity { get; }
    public StaggeredTransitionConfig StaggeredTransition { get; }

    public ConfigEntry<bool> VerboseLoggingEntry { get; }

    public NavigationConfig(ConfigFile cfg)
    {
        GlobalEnabledEntry = cfg.Bind("General", "Enabled", true,
            "Master toggle for all navigation strategies");

        PatrolPointOffset = new PatrolPointOffsetConfig(cfg);
        PatrolWayDiversity = new PatrolWayDiversityConfig(cfg);
        StaggeredTransition = new StaggeredTransitionConfig(cfg);

        VerboseLoggingEntry = cfg.Bind("Diagnostics", "VerboseLogging", false,
            "Emit per-strategy activation logs");
    }
}
