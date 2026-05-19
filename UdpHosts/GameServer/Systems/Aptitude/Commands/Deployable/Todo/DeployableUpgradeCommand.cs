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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}