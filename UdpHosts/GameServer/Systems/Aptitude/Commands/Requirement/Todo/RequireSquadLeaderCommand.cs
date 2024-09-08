using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireSquadLeaderCommand : Command, ICommand
{
    private RequireSquadLeaderCommandDef Params;

    public RequireSquadLeaderCommand(RequireSquadLeaderCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}