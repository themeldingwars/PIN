using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TeleportCommand : ICommand
{
    private TeleportCommandDef Params;

    public TeleportCommand(TeleportCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}