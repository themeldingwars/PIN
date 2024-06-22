using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetFilterByRangeCommand : ICommand
{
    private TargetFilterByRangeCommandDef Params;

    public TargetFilterByRangeCommand(TargetFilterByRangeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}