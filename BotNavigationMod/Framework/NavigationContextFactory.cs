using System.Collections.Generic;
using BotNavigationMod.Helpers;
using EFT;
using UnityEngine;

namespace BotNavigationMod.Framework;

public sealed class NavigationContextFactory
{
    private readonly NavigationContext _pooled = new NavigationContext();
    private readonly Dictionary<BotZone, PatrolWay[]> _zoneWayCache = new Dictionary<BotZone, PatrolWay[]>();
    private readonly Dictionary<int, float> _lastPointTimeByBot = new Dictionary<int, float>();
    private readonly HashSet<BotsGroup> _subscribedGroups = new HashSet<BotsGroup>();

    public NavigationContext Create(
        BotOwner bot,
        PatrollingData patrollingData,
        PatrolPointChooserBasic pointChooser,
        PatrolPointContainer proposedPoint,
        Vector3 proposedPosition)
    {
        var ctx = _pooled;
        ctx.Reset();

        ctx.Bot = bot;
        ctx.Group = bot.BotsGroup;
        ctx.PatrollingData = patrollingData;
        ctx.PointChooser = pointChooser;
        ctx.ProposedPoint = proposedPoint;
        ctx.ProposedPosition = proposedPosition;
        ctx.BotPosition = bot.Position;

        if (patrollingData != null)
        {
            ctx.Status = patrollingData.Status;
            ctx.CurrentWay = patrollingData.Way;
            if (pointChooser == null)
            {
                pointChooser = patrollingData.PointChooser;
                ctx.PointChooser = pointChooser;
            }
        }

        ctx.IsGroupPatrol = pointChooser is GClass562;
        ctx.ZoneWays = GetZoneWays(bot);
        ctx.GroupMemberCount = bot.BotsGroup != null ? bot.BotsGroup.MembersCount : 1;
        ctx.MemberIndex = BotGroupHelper.GetMemberIndex(bot);
        ctx.TimeSinceLastPoint = GetTimeSinceLastPoint(bot);

        SubscribeGroup(bot.BotsGroup);
        return ctx;
    }

    public void RecordComeToPoint(BotOwner bot)
    {
        if (bot == null)
        {
            return;
        }

        _lastPointTimeByBot[bot.Id] = Time.time;
    }

    public void RemoveBot(int botId)
    {
        _lastPointTimeByBot.Remove(botId);
    }

    private float GetTimeSinceLastPoint(BotOwner bot)
    {
        if (_lastPointTimeByBot.TryGetValue(bot.Id, out float lastTime))
        {
            return Time.time - lastTime;
        }

        if (bot.PatrollingData != null)
        {
            return Time.time - bot.PatrollingData.ComeToPointTime;
        }

        return 0f;
    }

    private PatrolWay[] GetZoneWays(BotOwner bot)
    {
        var zone = bot.BotsGroup?.BotZone;
        if (zone == null)
        {
            return null;
        }

        if (_zoneWayCache.TryGetValue(zone, out var cached))
        {
            return cached;
        }

        cached = zone.PatrolWays;
        _zoneWayCache[zone] = cached;
        return cached;
    }

    private void SubscribeGroup(BotsGroup group)
    {
        if (group == null || _subscribedGroups.Contains(group))
        {
            return;
        }

        group.OnMemberRemove += OnMemberRemoved;
        _subscribedGroups.Add(group);
    }

    private void OnMemberRemoved(BotOwner bot)
    {
        if (bot == null)
        {
            return;
        }

        NavigationInterceptor.Instance?.OnBotRemoved(bot.Id);
        RemoveBot(bot.Id);
    }
}
