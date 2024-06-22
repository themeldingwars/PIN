using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class DetonateProjectilesCommand : ICommand
{
    private DetonateProjectilesCommandDef Params;

    public DetonateProjectilesCommand(DetonateProjectilesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}