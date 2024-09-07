using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByHealthCommand : Command, ICommand
{
    private TargetByHealthCommandDef Params;

    public TargetByHealthCommand(TargetByHealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}