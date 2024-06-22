using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetByNPCCommand : ICommand
{
    private TargetByNPCCommandDef Params;

    public TargetByNPCCommand(TargetByNPCCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}