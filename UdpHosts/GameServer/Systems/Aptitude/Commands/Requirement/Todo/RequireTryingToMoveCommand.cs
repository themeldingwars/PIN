using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireTryingToMoveCommand : Command, ICommand
{
    private RequireTryingToMoveCommandDef Params;

    public RequireTryingToMoveCommand(RequireTryingToMoveCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}