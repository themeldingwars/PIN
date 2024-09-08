using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHasItemCommand : Command, ICommand
{
    private RequireHasItemCommandDef Params;

    public RequireHasItemCommand(RequireHasItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}