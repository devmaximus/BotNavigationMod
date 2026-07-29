using System.Collections.Generic;

namespace BotNavigationMod.Framework;

public sealed class StrategyRegistry
{
    private readonly List<INavigationStrategy> _strategies = new List<INavigationStrategy>();
    private readonly Dictionary<HookScope, INavigationStrategy[]> _byScope = new Dictionary<HookScope, INavigationStrategy[]>();

    public void Register(INavigationStrategy strategy)
    {
        _strategies.Add(strategy);
        RebuildScopeCache();
    }

    private void RebuildScopeCache()
    {
        _byScope.Clear();
        for (int s = 0; s < 4; s++)
        {
            var scope = (HookScope)s;
            var bucket = new List<INavigationStrategy>();
            for (int i = 0; i < _strategies.Count; i++)
            {
                if (_strategies[i].Scope == scope)
                {
                    bucket.Add(_strategies[i]);
                }
            }

            bucket.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            _byScope[scope] = bucket.ToArray();
        }
    }

    public NavigationResult Evaluate(HookScope scope, NavigationContext ctx)
    {
        if (!_byScope.TryGetValue(scope, out var sorted) || sorted == null)
        {
            return NavigationResult.PassThrough;
        }

        for (int i = 0; i < sorted.Length; i++)
        {
            var strategy = sorted[i];
            if (!strategy.IsEnabled)
            {
                continue;
            }

            if (!strategy.CanExecute(ctx))
            {
                continue;
            }

            var result = strategy.Execute(ctx);
            if (result.Action != NavigationAction.PassThrough)
            {
                return result;
            }
        }

        return NavigationResult.PassThrough;
    }
}
