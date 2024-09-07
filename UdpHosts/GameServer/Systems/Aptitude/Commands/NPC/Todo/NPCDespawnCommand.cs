using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NPCDespawnCommand : Command, ICommand
{
    private NPCDespawnCommandDef Params;

    public NPCDespawnCommand(NPCDespawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}