using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetSwapCommand : Command, ICommand
{
    private TargetSwapCommandDef Params;

    public TargetSwapCommand(TargetSwapCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        (context.Targets, context.FormerTargets) = (context.FormerTargets, context.Targets);

        if (Params.ClearCurrent == 1)
        {
            context.Targets.Clear();
        }

        if (Params.ClearFormer == 1)
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