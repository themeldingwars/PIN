using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireInRangeCommand : Command, ICommand
{
    private RequireInRangeCommandDef Params;

    public RequireInRangeCommand(RequireInRangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}