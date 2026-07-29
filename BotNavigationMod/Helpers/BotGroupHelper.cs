using EFT;

namespace BotNavigationMod.Helpers;

public static class BotGroupHelper
{
    public static int GetMemberIndex(BotOwner bot)
    {
        var group = bot?.BotsGroup;
        if (group == null)
        {
            return 0;
        }

        int count = group.MembersCount;
        for (int i = 0; i < count; i++)
        {
            if (group.Member(i) == bot)
            {
                return i;
            }
        }

        return 0;
    }

    public static bool IsGroupPatrolChooser(PatrolPointChooserBasic chooser)
    {
        return chooser is GClass562;
    }
}
