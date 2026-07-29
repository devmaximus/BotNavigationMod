using BotNavigationMod.Config;
using BotNavigationMod.Framework;
using BotNavigationMod.Helpers;
using UnityEngine;

namespace BotNavigationMod.Strategies;

public sealed class PatrolPointOffsetStrategy : INavigationStrategy
{
    public string Id => "S01";
    public int Priority => 100;
    public HookScope Scope => HookScope.GoToPoint;
    public bool IsEnabled => _config.IsEnabled;

    private readonly PatrolPointOffsetConfig _config;
    private readonly NavMeshHelper _navMeshHelper = new NavMeshHelper();

    public PatrolPointOffsetStrategy(PatrolPointOffsetConfig config)
    {
        _config = config;
    }

    public bool CanExecute(NavigationContext ctx)
    {
        return ctx.IsGroupPatrol
               && ctx.Status != PatrolStatus.pause
               && ctx.GroupMemberCount >= _config.GroupSizeThreshold
               && ctx.ProposedPosition != Vector3.zero;
    }

    public NavigationResult Execute(NavigationContext ctx)
    {
        Vector3 proposed = ctx.ProposedPoint != null ? ctx.ProposedPoint.Position : ctx.ProposedPosition;
        Vector3 direction = proposed - ctx.BotPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return NavigationResult.PassThrough;
        }

        direction.Normalize();
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        float side = ctx.MemberIndex % 2 == 0 ? 1f : -1f;
        float t = ctx.GroupMemberCount > 1
            ? ctx.MemberIndex / (float)ctx.GroupMemberCount
            : 0.5f;
        float magnitude = Mathf.Lerp(_config.MinOffset, _config.MaxOffset, t);
        Vector3 candidatePos = proposed + perpendicular * side * magnitude;

        if (!_navMeshHelper.TryGetReachablePosition(
                ctx.BotPosition,
                candidatePos,
                _config.SampleRadius,
                _config.MaxYDelta,
                out Vector3 reachable))
        {
            return NavigationResult.PassThrough;
        }

        return new NavigationResult
        {
            Action = NavigationAction.OverridePosition,
            OverridePosition = reachable
        };
    }
}
