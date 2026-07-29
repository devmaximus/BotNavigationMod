using EFT;
using UnityEngine;

namespace BotNavigationMod.Framework;

public sealed class NavigationContext
{
    public BotOwner Bot;
    public BotsGroup Group;
    public PatrollingData PatrollingData;
    public PatrolPointChooserBasic PointChooser;
    public PatrolWay CurrentWay;
    public PatrolPointContainer ProposedPoint;
    public Vector3 ProposedPosition;
    public Vector3 BotPosition;
    public PatrolStatus Status;
    public PatrolWay[] ZoneWays;
    public int GroupMemberCount;
    public float TimeSinceLastPoint;
    public int MemberIndex;
    public bool IsGroupPatrol;

    public void Reset()
    {
        Bot = null;
        Group = null;
        PatrollingData = null;
        PointChooser = null;
        CurrentWay = null;
        ProposedPoint = null;
        ProposedPosition = Vector3.zero;
        BotPosition = Vector3.zero;
        Status = PatrolStatus.stay;
        ZoneWays = null;
        GroupMemberCount = 0;
        TimeSinceLastPoint = 0f;
        MemberIndex = 0;
        IsGroupPatrol = false;
    }
}
