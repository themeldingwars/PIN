using System.Linq;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetInitiatorCommand : Command, ICommand
{
    private TargetInitiatorCommandDef Params;

    public TargetInitiatorCommand(TargetInitiatorCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Initiator;

        if (!context.Targets.Contains(target))
        {
            context.Targets.Push(target);
        }

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }
}