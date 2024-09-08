using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireFriendsCommand : Command, ICommand
{
    private RequireFriendsCommandDef Params;

    public RequireFriendsCommand(RequireFriendsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}