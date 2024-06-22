using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireSuperChargeCommand : ICommand
{
    private RequireSuperChargeCommandDef Params;

    public RequireSuperChargeCommand(RequireSuperChargeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}