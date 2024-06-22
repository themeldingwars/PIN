using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetOwnedDeployablesCommand : ICommand
{
    private TargetOwnedDeployablesCommandDef Params;

    public TargetOwnedDeployablesCommand(TargetOwnedDeployablesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}