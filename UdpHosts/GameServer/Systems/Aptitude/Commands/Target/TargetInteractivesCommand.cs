using GameServer.Entities;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetInteractivesCommand : Command, ICommand
{
    private TargetInteractivesCommandDef Params;

    public TargetInteractivesCommand(TargetInteractivesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        foreach (var target in context.Targets)
        {
            if (target is BaseEntity { Interaction: not null })
            {
                newTargets.Push(target);
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && newTargets.Count == 0)
        {
            return false;
        }

        return true;
    }
}