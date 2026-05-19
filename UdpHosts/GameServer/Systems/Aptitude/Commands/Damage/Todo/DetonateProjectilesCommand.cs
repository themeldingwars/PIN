using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class DetonateProjectilesCommand : Command, ICommand
{
    private DetonateProjectilesCommandDef Params;

    public DetonateProjectilesCommand(DetonateProjectilesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}