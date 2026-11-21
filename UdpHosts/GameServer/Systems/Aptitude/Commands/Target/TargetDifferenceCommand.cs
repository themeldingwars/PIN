using System.Collections.Generic;
using System.Linq;
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
        var current = SortedUnique(context.Targets);
        var former = SortedUnique(context.FormerTargets);
        var replaceFormer = Params.ReplaceFormer != 0;
        var swap = Params.SwapCurrentFormer != 0;

        var currentOnly = Except(current, former);
        var formerOnly = Except(former, current);

        context.Targets = new AptitudeTargets(swap ? formerOnly : currentOnly);
        context.FormerTargets = new AptitudeTargets(replaceFormer ? (swap ? currentOnly : formerOnly) : former);

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }

    private static IAptitudeTarget[] SortedUnique(IEnumerable<IAptitudeTarget> targets)
    {
        return targets.Where(t => t != null).GroupBy(t => t.EntityId).Select(g => g.First()).OrderBy(t => t.EntityId).ToArray();
    }

    private static IAptitudeTarget[] Except(IAptitudeTarget[] from, IAptitudeTarget[] remove)
    {
        var removeIds = remove.Select(t => t.EntityId).ToHashSet();
        return from.Where(t => !removeIds.Contains(t.EntityId)).ToArray();
    }
}