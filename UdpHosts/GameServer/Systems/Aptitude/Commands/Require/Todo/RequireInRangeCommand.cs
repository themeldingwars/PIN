using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireInRangeCommand : ICommand
{
    private RequireInRangeCommandDef Params;

    public RequireInRangeCommand(RequireInRangeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}