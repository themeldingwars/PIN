using System;
using System.Linq;
using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetSelfCommand : ICommand
{
    private TargetSelfCommandDef Params;

    public TargetSelfCommand(TargetSelfCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Console.WriteLine($"TargetSelfCommand Pre Target {context.Targets.FirstOrDefault()}");

        // TODO: FormerTargets ?
        context.Targets.Add(context.Self);

        Console.WriteLine($"TargetSelfCommand Post Target {context.Targets.FirstOrDefault()}");
        return true;
    }
}