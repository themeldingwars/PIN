using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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