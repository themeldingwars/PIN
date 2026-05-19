using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireInCombatCommand : Command, ICommand
{
    private RequireInCombatCommandDef Params;

    public RequireInCombatCommand(RequireInCombatCommandDef par)
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