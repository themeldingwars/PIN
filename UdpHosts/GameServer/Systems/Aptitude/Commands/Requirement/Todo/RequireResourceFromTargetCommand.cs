using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireResourceFromTargetCommand : Command, ICommand
{
    private RequireResourceFromTargetCommandDef Params;

    public RequireResourceFromTargetCommand(RequireResourceFromTargetCommandDef par)
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