using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class PeekTargetsCommand : Command, ICommand
{
    private PeekTargetsCommandDef Params;

    public PeekTargetsCommand(PeekTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        /* Former = 1 appears once in SDB, Current = 1 appears 107 times, they are mutually exclusive */

        if (Params.Former != 0)
        {
            if (context.TargetStack.Count == 0)
            {
                result.SetFail(StatusCode.Status1);
                return;
            }

            context.FormerTargets = new AptitudeTargets(context.TargetStack.Peek());
        }

        if (Params.Current != 0)
        {
            if (context.TargetStack.Count == 0)
            {
                result.SetFail(StatusCode.Status1);
                return;
            }

            context.Targets = new AptitudeTargets(context.TargetStack.Peek());
        }

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }
}