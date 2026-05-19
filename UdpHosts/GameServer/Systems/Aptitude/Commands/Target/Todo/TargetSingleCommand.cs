using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetSingleCommand : Command, ICommand
{
    private TargetSingleCommandDef Params;

    public TargetSingleCommand(TargetSingleCommandDef par)
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