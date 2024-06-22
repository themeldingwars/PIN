using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TeleportInstanceCommand : ICommand
{
    private TeleportInstanceCommandDef Params;

    public TeleportInstanceCommand(TeleportInstanceCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}