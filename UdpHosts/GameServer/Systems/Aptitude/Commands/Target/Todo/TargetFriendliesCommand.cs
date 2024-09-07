using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetFriendliesCommand : Command, ICommand
{
    private TargetFriendliesCommandDef Params;

    public TargetFriendliesCommand(TargetFriendliesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        foreach (var target in context.Targets)
        {
            // todo aptitude: remove non-friendlies
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