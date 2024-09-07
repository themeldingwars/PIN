using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetOwnedDeployablesCommand : Command, ICommand
{
    private TargetOwnedDeployablesCommandDef Params;

    public TargetOwnedDeployablesCommand(TargetOwnedDeployablesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}