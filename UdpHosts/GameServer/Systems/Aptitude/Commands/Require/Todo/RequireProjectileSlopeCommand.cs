using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireProjectileSlopeCommand : ICommand
{
    private RequireProjectileSlopeCommandDef Params;

    public RequireProjectileSlopeCommand(RequireProjectileSlopeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}