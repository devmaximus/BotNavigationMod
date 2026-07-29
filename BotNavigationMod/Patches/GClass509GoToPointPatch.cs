using BotNavigationMod.Framework;
using HarmonyLib;
using UnityEngine;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(GClass509), nameof(GClass509.GoToPoint))]
internal static class GClass509GoToPointPatch
{
    [HarmonyPrefix]
    private static void Prefix(GClass509 __instance, PatrolPointContainer point)
    {
        var interceptor = PatchContext.Interceptor;
        if (interceptor == null || __instance?.BotOwner_0?.PatrollingData == null || point == null)
        {
            return;
        }

        var patrollingData = __instance.BotOwner_0.PatrollingData;
        if (patrollingData.Status == PatrolStatus.pause)
        {
            return;
        }

        Vector3 target = point.Position;
        var result = interceptor.EvaluateGoToPoint(patrollingData, ref target);
        if (result.Action == NavigationAction.OverridePosition)
        {
            patrollingData.CurTargetPoint = target;
        }
    }
}
