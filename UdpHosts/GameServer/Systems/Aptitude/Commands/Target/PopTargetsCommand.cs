using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class PopTargetsCommand : Command, ICommand
{
    private PopTargetsCommandDef Params;

    public PopTargetsCommand(PopTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.Current == 0 && Params.Former == 0)
        {
            if (context.TargetStack.Count != 0)
            {
                context.TargetStack.Pop();
                result.SetPass(StatusCode.None);
                return;
            }
            else
            {
                Logger.Error("Targets stack underflow!");
                result.SetFail(StatusCode.Error);
                return;
            }
        }

        if (Params.Former != 0)
        {
            if (context.TargetStack.Count != 0)
            {
                context.FormerTargets = new AptitudeTargets(context.TargetStack.Pop());
            }
            else
            {
                Logger.Error("Targets stack underflow!");
                result.SetFail(StatusCode.Error);
                return;
            }
        }

        if (Params.Current != 0)
        {
            if (context.TargetStack.Count != 0)
            {
                context.Targets = new AptitudeTargets(context.TargetStack.Pop());
            }
            else
            {
                Logger.Error("Targets stack underflow!");
                result.SetFail(StatusCode.Error);
                return;
            }
        }

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }
}