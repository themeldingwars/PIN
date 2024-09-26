using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PushTargetsCommand : Command, ICommand
{
    private PushTargetsCommandDef Params;

    public PushTargetsCommand(PushTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo aptitude: verify what to push
        if (Params.Former == 1 && context.FormerTargets.Count > 0)
        {
            context.FormerTargets.Push(context.Targets.Peek());
        }

        if (Params.Current == 1 && context.Targets.Count > 0)
        {
            context.Targets.Push(context.Targets.Peek());
        }

        return true;
    }
}