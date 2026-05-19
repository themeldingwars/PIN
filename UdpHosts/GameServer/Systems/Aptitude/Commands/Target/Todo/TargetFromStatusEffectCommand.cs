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

    public bool Execute(Context context)
    {
        // todo aptitude
        if (Params.AlsoInitiator == 1)
        {
            context.Targets.Push(context.Initiator);
        }

        return true;
    }
}