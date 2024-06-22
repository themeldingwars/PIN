using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetByNPCTypeCommand : ICommand
{
    private TargetByNPCTypeCommandDef Params;

    public TargetByNPCTypeCommand(TargetByNPCTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}