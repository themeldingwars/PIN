using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireFriendsCommand : ICommand
{
    private RequireFriendsCommandDef Params;

    public RequireFriendsCommand(RequireFriendsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}