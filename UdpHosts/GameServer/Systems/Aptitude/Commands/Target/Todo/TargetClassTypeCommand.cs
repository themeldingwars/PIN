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

    public bool Execute(Context context)
    {
        return true;
    }
}