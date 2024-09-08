using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TeleportInstanceCommand : Command, ICommand
{
    private TeleportInstanceCommandDef Params;

    public TeleportInstanceCommand(TeleportInstanceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}