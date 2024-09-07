using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetFilterByRangeCommand : Command, ICommand
{
    private TargetFilterByRangeCommandDef Params;

    public TargetFilterByRangeCommand(TargetFilterByRangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}