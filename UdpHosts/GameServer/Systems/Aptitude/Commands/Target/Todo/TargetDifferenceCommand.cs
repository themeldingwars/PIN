using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetDifferenceCommand : Command, ICommand
{
    private TargetDifferenceCommandDef Params;

    public TargetDifferenceCommand(TargetDifferenceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // todo aptitude: target difference
        if (Params.ReplaceFormer == 1)
        {
            context.FormerTargets = context.Targets;
        }

        if (Params.SwapCurrentFormer == 1)
        {
            (context.Targets, context.FormerTargets) = (context.FormerTargets, context.Targets);
        }
    }

    public override void Reset(Context context)
    {
        return;
    }
}