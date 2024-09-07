using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireProjectileSlopeCommand : Command, ICommand
{
    private RequireProjectileSlopeCommandDef Params;

    public RequireProjectileSlopeCommand(RequireProjectileSlopeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}