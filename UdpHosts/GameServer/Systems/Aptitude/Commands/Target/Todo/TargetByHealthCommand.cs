using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByHealthCommand : ICommand
{
    private TargetByHealthCommandDef Params;

    public TargetByHealthCommand(TargetByHealthCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}