using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireItemDurabilityCommand : ICommand
{
    private RequireItemDurabilityCommandDef Params;

    public RequireItemDurabilityCommand(RequireItemDurabilityCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}