using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireEquippedItemCommand : Command, ICommand
{
    private RequireEquippedItemCommandDef Params;

    public RequireEquippedItemCommand(RequireEquippedItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}