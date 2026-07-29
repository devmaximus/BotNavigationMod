using HarmonyLib;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(PatrollingData), nameof(PatrollingData.ComeToPoint))]
internal static class PatrollingDataComeToPointPatch
{
    [HarmonyPostfix]
    private static void Postfix(PatrollingData __instance)
    {
        if (__instance == null)
        {
            return;
        }

        PatchContext.Interceptor?.OnComeToPoint(__instance);
    }
}
