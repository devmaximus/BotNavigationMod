using BotNavigationMod.Framework;
using HarmonyLib;
using UnityEngine;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(PatrollingData), nameof(PatrollingData.GoToPoint))]
internal static class PatrollingDataGoToPointPatch
{
    [HarmonyPrefix]
    private static void Prefix(PatrollingData __instance)
    {
        var interceptor = PatchContext.Interceptor;
        if (interceptor == null || __instance == null)
        {
            return;
        }

        Vector3 target = __instance.CurTargetPoint;
        var result = interceptor.EvaluateGoToPoint(__instance, ref target);
        if (result.Action == NavigationAction.OverridePosition)
        {
            __instance.CurTargetPoint = target;
        }
    }
}
