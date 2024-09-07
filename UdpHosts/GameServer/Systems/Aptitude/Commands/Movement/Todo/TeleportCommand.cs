using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TeleportCommand : Command, ICommand
{
    private TeleportCommandDef Params;

    public TeleportCommand(TeleportCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}