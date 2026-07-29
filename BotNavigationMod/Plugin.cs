using System;
using BotNavigationMod.Framework;
using BotNavigationMod.Patches;
using BotNavigationMod.Strategies;
using BepInEx;
using HarmonyLib;

namespace BotNavigationMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.devmaximus.botnavigationmod";
    public const string PluginName = "BotNavigationMod";
    public const string PluginVersion = "1.0.0";

    private static BepInEx.Logging.ManualLogSource _log;
    private NavigationInterceptor _interceptor;
    private StaggeredTransitionStrategy _staggeredTransition;

    public static void LogInfoStatic(string message)
    {
        _log?.LogInfo(message);
    }

    private void Awake()
    {
        _log = Logger;
        var config = new Config.NavigationConfig(Config);
        var registry = new StrategyRegistry();
        var contextFactory = new NavigationContextFactory();

        _staggeredTransition = new StaggeredTransitionStrategy(config.StaggeredTransition);
        registry.Register(new PatrolPointOffsetStrategy(config.PatrolPointOffset));
        registry.Register(new PatrolWayDiversityStrategy(config.PatrolWayDiversity));
        registry.Register(_staggeredTransition);

        _interceptor = new NavigationInterceptor(registry, contextFactory, config, _staggeredTransition);
        PatchContext.Initialize(_interceptor, _staggeredTransition, config);

        var harmony = new Harmony(PluginGuid);
        harmony.PatchAll(typeof(PatrollingDataGoToPointPatch));
        harmony.PatchAll(typeof(PatrollingDataComeToPointPatch));
        harmony.PatchAll(typeof(GClass562FindNextPointPatch));
        harmony.PatchAll(typeof(PatrolPointChooserBasicChooseStartWayPatch));
        harmony.PatchAll(typeof(GClass562TryToFindWayPatch));
        harmony.PatchAll(typeof(BotOwnerGoToPointPatch));
        harmony.PatchAll(typeof(GClass509GoToPointPatch));

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded — S01+S02+S03 active");
    }
}
