using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class PushTargetsCommand : Command, ICommand
{
    private PushTargetsCommandDef Params;

    public PushTargetsCommand(PushTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    // TODO: Should this reset the context targets lists after push?
    // NOTE: Target stack overflow does not affect result code
    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.Current != 0)
        {
            if (context.TargetStack.Count < 101)
            {
                var copy = new AptitudeTargets(context.Targets);
                context.TargetStack.Push(copy);
            }
            else
            {
                Logger.Error("Target stack overflow");
            }
        }

        if (Params.Former != 0)
        {
            if (context.TargetStack.Count < 100)
            {
                var copy = new AptitudeTargets(context.FormerTargets);
                context.TargetStack.Push(copy);
            }
            else
            {
                Logger.Error("Target stack overflow");
            }
        }

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }
}