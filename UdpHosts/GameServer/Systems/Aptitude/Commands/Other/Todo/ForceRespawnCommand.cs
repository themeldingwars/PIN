using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ForceRespawnCommand : Command, ICommand
{
    private ForceRespawnCommandDef Params;

    public ForceRespawnCommand(ForceRespawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}