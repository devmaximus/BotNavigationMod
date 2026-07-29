using BotNavigationMod.Framework;
using HarmonyLib;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(PatrolPointChooserBasic), nameof(PatrolPointChooserBasic.ChooseStartWay))]
internal static class PatrolPointChooserBasicChooseStartWayPatch
{
    [HarmonyPrefix]
    private static bool Prefix(PatrolPointChooserBasic __instance)
    {
        var interceptor = PatchContext.Interceptor;
        if (interceptor == null || __instance?.Owner == null)
        {
            return true;
        }

        var result = interceptor.EvaluateChooseStartWay(__instance);
        if (result.Action != NavigationAction.OverrideWay || result.OverrideWay == null)
        {
            return true;
        }

        __instance.PointControl.SetWay(result.OverrideWay, __instance.FindNextPoint);
        return false;
    }
}
