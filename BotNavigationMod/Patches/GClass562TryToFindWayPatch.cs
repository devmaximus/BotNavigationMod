using BotNavigationMod.Framework;
using HarmonyLib;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(GClass562), nameof(GClass562.TryToFindWay))]
internal static class GClass562TryToFindWayPatch
{
    [HarmonyPrefix]
    private static bool Prefix(GClass562 __instance, ref PatrolWay way, ref float delta, ref bool __result)
    {
        var interceptor = PatchContext.Interceptor;
        if (interceptor == null || __instance?.Owner == null)
        {
            return true;
        }

        var navResult = interceptor.EvaluateChooseStartWay(__instance);
        if (navResult.Action != NavigationAction.OverrideWay || navResult.OverrideWay == null)
        {
            return true;
        }

        way = navResult.OverrideWay;
        delta = __instance.Owner.Settings.FileSettings.Patrol.CHANGE_WAY_TIME;
        __instance.NextChangeWay = UnityEngine.Time.time + delta * GClass856.Random(0.6f, 1.4f);
        __result = true;
        return false;
    }
}
