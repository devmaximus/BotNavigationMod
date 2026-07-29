namespace BotNavigationMod.Framework;

public interface INavigationStrategy
{
    string Id { get; }
    int Priority { get; }
    HookScope Scope { get; }
    bool IsEnabled { get; }
    bool CanExecute(NavigationContext ctx);
    NavigationResult Execute(NavigationContext ctx);
}
