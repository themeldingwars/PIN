using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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