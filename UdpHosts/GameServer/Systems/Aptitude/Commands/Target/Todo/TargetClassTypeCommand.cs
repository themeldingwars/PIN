using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetClassTypeCommand : Command, ICommand
{
    private TargetClassTypeCommandDef Params;

    public TargetClassTypeCommand(TargetClassTypeCommandDef par)
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