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

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}