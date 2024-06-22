using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class OverrideCollisionCommand : ICommand
{
    private OverrideCollisionCommandDef Params;

    public OverrideCollisionCommand(OverrideCollisionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}