using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

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