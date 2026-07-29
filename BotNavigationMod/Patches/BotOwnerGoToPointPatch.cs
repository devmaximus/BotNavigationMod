using BotNavigationMod.Framework;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(BotOwner), nameof(BotOwner.GoToPoint), typeof(Vector3), typeof(bool), typeof(float),
    typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
internal static class BotOwnerGoToPointPatch
{
    [HarmonyPrefix]
    private static void Prefix(BotOwner __instance, ref Vector3 position)
    {
        var interceptor = PatchContext.Interceptor;
        if (interceptor == null || __instance?.PatrollingData == null)
        {
            return;
        }

        if (__instance.PatrollingData.Status == PatrolStatus.pause)
        {
            return;
        }

        var patrollingData = __instance.PatrollingData;
        interceptor.EvaluateGoToPoint(patrollingData, ref position);
    }
}
