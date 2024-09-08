using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireItemDurabilityCommand : Command, ICommand
{
    private RequireItemDurabilityCommandDef Params;

    public RequireItemDurabilityCommand(RequireItemDurabilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}