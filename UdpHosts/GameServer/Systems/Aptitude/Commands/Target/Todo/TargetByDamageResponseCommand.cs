using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByDamageResponseCommand : Command, ICommand
{
    private TargetByDamageResponseCommandDef Params;

    public TargetByDamageResponseCommand(TargetByDamageResponseCommandDef par)
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