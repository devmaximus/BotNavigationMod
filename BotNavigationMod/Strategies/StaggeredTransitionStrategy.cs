using System.Collections.Generic;
using BotNavigationMod.Config;
using BotNavigationMod.Framework;
using EFT;
using UnityEngine;

namespace BotNavigationMod.Strategies;

public sealed class StaggeredTransitionStrategy : INavigationStrategy
{
    public string Id => "S03";
    public int Priority => 200;
    public HookScope Scope => HookScope.FindNextPoint;
    public bool IsEnabled => _config.IsEnabled;

    private readonly StaggeredTransitionConfig _config;
    private readonly Dictionary<int, float> _delayExpiry = new Dictionary<int, float>();

    public StaggeredTransitionStrategy(StaggeredTransitionConfig config)
    {
        _config = config;
    }

    public bool CanExecute(NavigationContext ctx)
    {
        return ctx.GroupMemberCount >= 2
               && ctx.Status != PatrolStatus.pause;
    }

    public NavigationResult Execute(NavigationContext ctx)
    {
        int botId = ctx.Bot.Id;

        if (_delayExpiry.TryGetValue(botId, out float expiry))
        {
            if (Time.time < expiry)
            {
                return new NavigationResult
                {
                    Action = NavigationAction.Delay,
                    Delay = expiry - Time.time
                };
            }

            _delayExpiry.Remove(botId);
            return NavigationResult.PassThrough;
        }

        float totalDelay = ComputeDelay(ctx.MemberIndex);
        if (totalDelay < 0.1f)
        {
            return NavigationResult.PassThrough;
        }

        _delayExpiry[botId] = Time.time + totalDelay;
        return new NavigationResult
        {
            Action = NavigationAction.Delay,
            Delay = totalDelay
        };
    }

    public void CleanupBot(int botId)
    {
        _delayExpiry.Remove(botId);
        SweepExpired();
    }

    private float ComputeDelay(int memberIndex)
    {
        float jitter = Random.Range(0f, _config.MaxJitter);
        float total = _config.BaseDelay + memberIndex * _config.PerMemberDelay + jitter;
        return Mathf.Min(total, _config.MaxTotalDelay);
    }

    private void SweepExpired()
    {
        if (_delayExpiry.Count < 64)
        {
            return;
        }

        float now = Time.time;
        var toRemove = new List<int>(8);
        foreach (var pair in _delayExpiry)
        {
            if (now >= pair.Value)
            {
                toRemove.Add(pair.Key);
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            _delayExpiry.Remove(toRemove[i]);
        }
    }
}
