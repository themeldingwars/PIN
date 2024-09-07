using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetByExistsCommand : Command, ICommand
{
    private TargetByExistsCommandDef Params;

    public TargetByExistsCommand(TargetByExistsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}