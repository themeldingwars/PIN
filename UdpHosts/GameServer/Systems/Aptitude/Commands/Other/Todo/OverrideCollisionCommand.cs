using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class OverrideCollisionCommand : Command, ICommand
{
    private OverrideCollisionCommandDef Params;

    public OverrideCollisionCommand(OverrideCollisionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}