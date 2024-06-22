using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NPCDespawnCommand : ICommand
{
    private NPCDespawnCommandDef Params;

    public NPCDespawnCommand(NPCDespawnCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}