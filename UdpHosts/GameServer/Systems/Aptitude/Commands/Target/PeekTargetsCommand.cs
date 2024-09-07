using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PeekTargetsCommand : Command, ICommand
{
    private PeekTargetsCommandDef Params;

    public PeekTargetsCommand(PeekTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        /* Former = 1 appears once in SDB, Current = 1 appears 107 times, they are mutually exclusive */

        if (Params.Former == 1)
        {
            var ok = context.FormerTargets.TryPeek(out _);

            return ok;
        }

        if (Params.Current == 1)
        {
            var ok = context.Targets.TryPeek(out _);

            return ok;
        }

        return true;
    }
}