using System;
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
            // assuming push == saving for later, this shouldnt occur and it doesnt in 1962
            Console.WriteLine($"[PushTargets] Former = 1, FormerTargets count {context.FormerTargets.Count}");
        }

        if (Params.Current == 1)
        {
            context.FormerTargets = new AptitudeTargets(context.Targets);
            context.Targets = new AptitudeTargets();
        }

        return true;
    }
}