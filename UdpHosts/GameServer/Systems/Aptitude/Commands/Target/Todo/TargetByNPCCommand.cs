using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByNPCCommand : Command, ICommand
{
    private TargetByNPCCommandDef Params;

    public TargetByNPCCommand(TargetByNPCCommandDef par)
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