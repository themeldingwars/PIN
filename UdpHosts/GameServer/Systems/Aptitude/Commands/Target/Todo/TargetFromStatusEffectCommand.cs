using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetFromStatusEffectCommand : Command, ICommand
{
    private TargetFromStatusEffectCommandDef Params;

    public TargetFromStatusEffectCommand(TargetFromStatusEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // todo aptitude
        if (Params.AlsoInitiator == 1)
        {
            context.Targets.Push(context.Initiator);
        }
    }

    public override void Reset(Context context)
    {
        return;
    }
}