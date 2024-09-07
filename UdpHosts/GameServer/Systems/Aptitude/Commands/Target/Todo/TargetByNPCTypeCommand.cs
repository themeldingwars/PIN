using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetByNPCTypeCommand : Command, ICommand
{
    private TargetByNPCTypeCommandDef Params;

    public TargetByNPCTypeCommand(TargetByNPCTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}