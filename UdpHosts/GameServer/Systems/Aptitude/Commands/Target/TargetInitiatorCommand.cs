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

    public bool Execute(Context context)
    {
        context.FormerTargets = new AptitudeTargets(context.Targets);
        context.Targets.Push(context.Initiator);

        return true;
    }
}