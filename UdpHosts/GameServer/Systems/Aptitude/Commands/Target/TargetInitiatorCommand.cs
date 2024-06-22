using System;
using System.Linq;
using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetInitiatorCommand : ICommand
{
    private TargetInitiatorCommandDef Params;

    public TargetInitiatorCommand(TargetInitiatorCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // Console.WriteLine($"TargetSelfCommand Pre Target {context.Targets.FirstOrDefault()}");

        // TODO: FormerTargets ?
        context.Targets.Push(context.Initiator);

        // Console.WriteLine($"TargetSelfCommand Post Target {context.Targets.FirstOrDefault()}");
        return true;
    }
}