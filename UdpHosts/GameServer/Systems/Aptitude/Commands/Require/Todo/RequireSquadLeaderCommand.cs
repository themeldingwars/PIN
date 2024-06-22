using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireSquadLeaderCommand : ICommand
{
    private RequireSquadLeaderCommandDef Params;

    public RequireSquadLeaderCommand(RequireSquadLeaderCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}