using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHasItemCommand : ICommand
{
    private RequireHasItemCommandDef Params;

    public RequireHasItemCommand(RequireHasItemCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}