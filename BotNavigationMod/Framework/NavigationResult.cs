using UnityEngine;

namespace BotNavigationMod.Framework;

public struct NavigationResult
{
    public NavigationAction Action;
    public Vector3 OverridePosition;
    public PatrolPointContainer OverridePoint;
    public PatrolWay OverrideWay;
    public float Delay;

    public static NavigationResult PassThrough =>
        new NavigationResult { Action = NavigationAction.PassThrough };
}
