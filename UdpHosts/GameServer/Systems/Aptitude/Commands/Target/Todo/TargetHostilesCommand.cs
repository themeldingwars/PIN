using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetHostilesCommand : Command, ICommand
{
    private TargetHostilesCommandDef Params;

    public TargetHostilesCommand(TargetHostilesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        foreach (var target in context.Targets)
        {
            // todo aptitude: remove friendlies
        }

        if (Params.IncludeInitiator == 1)
        {
            context.Targets.Push(context.Initiator);
        }

        if (Params.IncludeOwner == 1)
        {
            // todo aptitude: handle owner
        }

        if (Params.IncludeSelf == 1)
        {
            context.Targets.Push(context.Self);
        }

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}