using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

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