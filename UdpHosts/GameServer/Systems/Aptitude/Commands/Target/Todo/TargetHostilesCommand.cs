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
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        foreach (var target in previousTargets)
        {
            // add  || (target is friendly)
            if (
                (target == context.Self && Params.IncludeSelf == 0)
                || (target == context.Initiator && Params.IncludeInitiator == 0)
                || (target == context.Self.Owner && Params.IncludeOwner == 0))
            {
                continue;
            }

            context.Targets.Push(target);
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}