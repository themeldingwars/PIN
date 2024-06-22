using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetClassTypeCommand : ICommand
{
    private TargetClassTypeCommandDef Params;

    public TargetClassTypeCommand(TargetClassTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}