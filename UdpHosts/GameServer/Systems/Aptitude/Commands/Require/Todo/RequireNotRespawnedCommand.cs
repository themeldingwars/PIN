using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireNotRespawnedCommand : ICommand
{
    private RequireNotRespawnedCommandDef Params;

    public RequireNotRespawnedCommand(RequireNotRespawnedCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}