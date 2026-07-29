using BotNavigationMod.Config;
using BotNavigationMod.Framework;
using BotNavigationMod.Strategies;

namespace BotNavigationMod.Patches;

internal static class PatchContext
{
    public static NavigationInterceptor Interceptor { get; private set; }
    public static StaggeredTransitionStrategy StaggeredTransition { get; private set; }
    public static NavigationConfig Config { get; private set; }

    public static void Initialize(
        NavigationInterceptor interceptor,
        StaggeredTransitionStrategy staggeredTransition,
        NavigationConfig config)
    {
        Interceptor = interceptor;
        StaggeredTransition = staggeredTransition;
        Config = config;
    }
}
