using System.Collections.Generic;
using BotNavigationMod.Config;
using BotNavigationMod.Framework;
using EFT;
using UnityEngine;

namespace BotNavigationMod.Strategies;

public sealed class PatrolWayDiversityStrategy : INavigationStrategy
{
    public string Id => "S02";
    public int Priority => 50;
    public HookScope Scope => HookScope.ChooseStartWay;
    public bool IsEnabled => _config.IsEnabled;

    private readonly PatrolWayDiversityConfig _config;
    private readonly Dictionary<BotZone, bool> _insufficientWaysLogged = new Dictionary<BotZone, bool>();
    private readonly Dictionary<BotsGroup, float> _lastRebalanceTime = new Dictionary<BotsGroup, float>();
    private readonly List<PatrolWay> _candidateBuffer = new List<PatrolWay>(16);
    private readonly int[] _usageScratch = new int[32];

    public PatrolWayDiversityStrategy(PatrolWayDiversityConfig config)
    {
        _config = config;
    }

    public bool CanExecute(NavigationContext ctx)
    {
        return ctx.ZoneWays != null
               && ctx.ZoneWays.Length >= 2
               && ctx.GroupMemberCount >= 2
               && ctx.Bot != null;
    }

    public NavigationResult Execute(NavigationContext ctx)
    {
        var ways = ctx.ZoneWays;
        if (ways == null || ways.Length < 2)
        {
            LogInsufficientWaysOnce(ctx.Bot.BotsGroup?.BotZone, ways?.Length ?? 0);
            return NavigationResult.PassThrough;
        }

        var group = ctx.Group;
        if (group != null)
        {
            if (_lastRebalanceTime.TryGetValue(group, out float last) && Time.time - last < _config.RebalanceInterval)
            {
                if (ctx.CurrentWay != null && CountWayUsage(group, ways, ctx.CurrentWay) < _config.MaxBotsPerWay)
                {
                    return NavigationResult.PassThrough;
                }
            }
        }

        BuildUsageMap(group, ways);
        int currentUsage = ctx.CurrentWay != null ? GetUsageForWay(ways, ctx.CurrentWay) : 0;
        if (currentUsage < _config.MaxBotsPerWay && ctx.CurrentWay != null)
        {
            return NavigationResult.PassThrough;
        }

        PatrolWay selected = SelectLeastUsedWay(ctx, ways);
        if (selected == null || selected == ctx.CurrentWay)
        {
            return NavigationResult.PassThrough;
        }

        if (group != null)
        {
            _lastRebalanceTime[group] = Time.time;
        }

        return new NavigationResult
        {
            Action = NavigationAction.OverrideWay,
            OverrideWay = selected
        };
    }

    private void BuildUsageMap(BotsGroup group, PatrolWay[] ways)
    {
        for (int i = 0; i < _usageScratch.Length; i++)
        {
            _usageScratch[i] = 0;
        }

        if (group == null)
        {
            return;
        }

        int memberCount = group.MembersCount;
        for (int m = 0; m < memberCount; m++)
        {
            BotOwner member = group.Member(m);
            if (member?.PatrollingData?.Way == null)
            {
                continue;
            }

            int wayIndex = IndexOfWay(ways, member.PatrollingData.Way);
            if (wayIndex >= 0 && wayIndex < _usageScratch.Length)
            {
                _usageScratch[wayIndex]++;
            }
        }
    }

    private static int IndexOfWay(PatrolWay[] ways, PatrolWay way)
    {
        for (int i = 0; i < ways.Length; i++)
        {
            if (ways[i] == way)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetUsageForWay(PatrolWay[] ways, PatrolWay way)
    {
        int index = IndexOfWay(ways, way);
        if (index < 0 || index >= _usageScratch.Length)
        {
            return 0;
        }

        return _usageScratch[index];
    }

    private static int CountWayUsage(BotsGroup group, PatrolWay[] ways, PatrolWay target)
    {
        int count = 0;
        int memberCount = group.MembersCount;
        for (int m = 0; m < memberCount; m++)
        {
            BotOwner member = group.Member(m);
            if (member?.PatrollingData?.Way == target)
            {
                count++;
            }
        }

        return count;
    }

    private PatrolWay SelectLeastUsedWay(NavigationContext ctx, PatrolWay[] ways)
    {
        _candidateBuffer.Clear();
        BotOwner bot = ctx.Bot;

        for (int i = 0; i < ways.Length; i++)
        {
            PatrolWay way = ways[i];
            if (way == null)
            {
                continue;
            }

            if (way.PatrolType == PatrolType.boss || way.PatrolType == PatrolType.reserved)
            {
                continue;
            }

            if (!way.HaveFreeSpace(bot))
            {
                continue;
            }

            int usage = i < _usageScratch.Length ? _usageScratch[i] : 0;
            if (usage >= _config.MaxBotsPerWay)
            {
                continue;
            }

            _candidateBuffer.Add(way);
        }

        if (_candidateBuffer.Count == 0)
        {
            return null;
        }

        PatrolWay best = null;
        int bestUsage = int.MaxValue;
        float bestScore = float.MaxValue;

        for (int c = 0; c < _candidateBuffer.Count; c++)
        {
            PatrolWay way = _candidateBuffer[c];
            int index = IndexOfWay(ways, way);
            int usage = index >= 0 && index < _usageScratch.Length ? _usageScratch[index] : 0;

            float distanceScore = 0f;
            if (_config.PreferNearbyWays)
            {
                Vector3 centroid = way.Vector3_0;
                distanceScore = (centroid - ctx.BotPosition).sqrMagnitude * _config.DistanceWeight;
            }

            float score = usage + distanceScore;
            if (usage < bestUsage || (usage == bestUsage && score < bestScore))
            {
                bestUsage = usage;
                bestScore = score;
                best = way;
            }
        }

        return best;
    }

    private void LogInsufficientWaysOnce(BotZone zone, int count)
    {
        if (zone == null || count >= 2)
        {
            return;
        }

        if (_insufficientWaysLogged.ContainsKey(zone))
        {
            return;
        }

        _insufficientWaysLogged[zone] = true;
        Plugin.LogInfoStatic($"[S02] Zone '{zone.NameZone}': {count} PatrolWays — S02 inactive");
    }
}
