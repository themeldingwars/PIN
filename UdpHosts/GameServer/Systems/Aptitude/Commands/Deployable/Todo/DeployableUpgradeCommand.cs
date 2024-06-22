using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DeployableUpgradeCommand : ICommand
{
    private DeployableUpgradeCommandDef Params;

    public DeployableUpgradeCommand(DeployableUpgradeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}