using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireAbilityPhysicsCommand : Command, ICommand
{
    private RequireAbilityPhysicsCommandDef Params;

    public RequireAbilityPhysicsCommand(RequireAbilityPhysicsCommandDef par)
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