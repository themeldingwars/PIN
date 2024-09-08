using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireNotRespawnedCommand : Command, ICommand
{
    private RequireNotRespawnedCommandDef Params;

    public RequireNotRespawnedCommand(RequireNotRespawnedCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}