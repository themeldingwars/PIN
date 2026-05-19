using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetClearCommand : Command, ICommand
{
    private TargetClearCommandDef Params;

    public TargetClearCommand(TargetClearCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.Current == 1)
        {
            context.Targets.Clear();

            // TODO: Additional free logic?
        }

        if (Params.Former == 1)
        {
            context.FormerTargets.Clear();
        }

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }
}