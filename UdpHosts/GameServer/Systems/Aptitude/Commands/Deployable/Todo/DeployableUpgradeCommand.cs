using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude;

public class DeployableUpgradeCommand : Command, ICommand
{
    private DeployableUpgradeCommandDef Params;

    public DeployableUpgradeCommand(DeployableUpgradeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}