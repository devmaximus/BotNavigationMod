using System.Collections.Generic;
using BotNavigationMod.Config;
using BotNavigationMod.Strategies;
using EFT;
using UnityEngine;

namespace BotNavigationMod.Framework;

public sealed class NavigationInterceptor
{
    public static NavigationInterceptor Instance { get; private set; }

    private const float LastTargetEpsilonSqr = 1f;

    private readonly StrategyRegistry _registry;
    private readonly NavigationContextFactory _contextFactory;
    private readonly NavigationConfig _config;
    private readonly StaggeredTransitionStrategy _staggeredTransition;
    private readonly Dictionary<int, Vector3> _lastEvaluatedTarget = new Dictionary<int, Vector3>();

    public NavigationConfig Config => _config;

    public NavigationInterceptor(
        StrategyRegistry registry,
        NavigationContextFactory contextFactory,
        NavigationConfig config,
        StaggeredTransitionStrategy staggeredTransition)
    {
        _registry = registry;
        _contextFactory = contextFactory;
        _config = config;
        _staggeredTransition = staggeredTransition;
        Instance = this;
    }

    public NavigationResult EvaluateGoToPoint(PatrollingData patrollingData, ref Vector3 targetPosition)
    {
        if (!_config.GlobalEnabledEntry.Value || patrollingData == null)
        {
            return NavigationResult.PassThrough;
        }

        if (patrollingData.Status == PatrolStatus.pause)
        {
            return NavigationResult.PassThrough;
        }

        var bot = patrollingData.BotOwner_0;
        if (ShouldSkipDuplicateTarget(bot, targetPosition))
        {
            return NavigationResult.PassThrough;
        }

        var ctx = _contextFactory.Create(
            bot,
            patrollingData,
            patrollingData.PointChooser,
            patrollingData.CurPatrolPoint,
            targetPosition);

        var result = _registry.Evaluate(HookScope.GoToPoint, ctx);
        if (result.Action == NavigationAction.OverridePosition)
        {
            targetPosition = result.OverridePosition;
            RememberTarget(bot, targetPosition);
        }
        else if (result.Action != NavigationAction.PassThrough)
        {
            RememberTarget(bot, targetPosition);
        }

        return result;
    }

    public NavigationResult EvaluateFindNextPoint(GClass562 chooser)
    {
        if (!_config.GlobalEnabledEntry.Value || chooser?.Owner == null)
        {
            return NavigationResult.PassThrough;
        }

        var bot = chooser.Owner;
        var patrollingData = bot.PatrollingData;
        if (patrollingData == null || patrollingData.Status == PatrolStatus.pause)
        {
            return NavigationResult.PassThrough;
        }

        var ctx = _contextFactory.Create(bot, patrollingData, chooser, null, Vector3.zero);
        return _registry.Evaluate(HookScope.FindNextPoint, ctx);
    }

    public NavigationResult EvaluateChooseStartWay(PatrolPointChooserBasic chooser)
    {
        if (!_config.GlobalEnabledEntry.Value || chooser?.Owner == null)
        {
            return NavigationResult.PassThrough;
        }

        var bot = chooser.Owner;
        var patrollingData = bot.PatrollingData;
        if (patrollingData == null || patrollingData.Status == PatrolStatus.pause)
        {
            return NavigationResult.PassThrough;
        }

        var ctx = _contextFactory.Create(bot, patrollingData, chooser, null, Vector3.zero);
        return _registry.Evaluate(HookScope.ChooseStartWay, ctx);
    }

    public void OnComeToPoint(PatrollingData patrollingData)
    {
        if (!_config.GlobalEnabledEntry.Value || patrollingData == null)
        {
            return;
        }

        var bot = patrollingData.BotOwner_0;
        _contextFactory.RecordComeToPoint(bot);

        var ctx = _contextFactory.Create(
            bot,
            patrollingData,
            patrollingData.PointChooser,
            patrollingData.CurPatrolPoint,
            patrollingData.CurTargetPoint);

        _registry.Evaluate(HookScope.ComeToPoint, ctx);
        _lastEvaluatedTarget.Remove(bot.Id);
    }

    public void OnBotRemoved(int botId)
    {
        _lastEvaluatedTarget.Remove(botId);
        _contextFactory.RemoveBot(botId);
        _staggeredTransition?.CleanupBot(botId);
    }

    private bool ShouldSkipDuplicateTarget(BotOwner bot, Vector3 targetPosition)
    {
        if (bot == null)
        {
            return false;
        }

        if (_lastEvaluatedTarget.TryGetValue(bot.Id, out var last))
        {
            if ((targetPosition - last).sqrMagnitude < LastTargetEpsilonSqr)
            {
                return true;
            }
        }

        return false;
    }

    private void RememberTarget(BotOwner bot, Vector3 targetPosition)
    {
        if (bot != null)
        {
            _lastEvaluatedTarget[bot.Id] = targetPosition;
        }
    }
}
