using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetSquadmatesCommand : Command, ICommand
{
    private TargetSquadmatesCommandDef Params;

    public TargetSquadmatesCommand(TargetSquadmatesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}