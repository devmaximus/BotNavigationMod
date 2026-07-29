using System;
using BotNavigationMod.Framework;
using HarmonyLib;

namespace BotNavigationMod.Patches;

[HarmonyPatch(typeof(GClass562), nameof(GClass562.FindNextPoint))]
[HarmonyPatch(new Type[]
{
    typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(GDelegate4)
})]
internal static class GClass562FindNextPointPatch
{
    [HarmonyPrefix]
    private static bool Prefix(GClass562 __instance, ref PatrolPointContainer __result)
    {
        var interceptor = PatchContext.Interceptor;
        if (interceptor == null || __instance == null)
        {
            return true;
        }

        var result = interceptor.EvaluateFindNextPoint(__instance);
        if (result.Action == NavigationAction.Delay || result.Action == NavigationAction.Skip)
        {
            __result = null;
            return false;
        }

        return true;
    }
}
