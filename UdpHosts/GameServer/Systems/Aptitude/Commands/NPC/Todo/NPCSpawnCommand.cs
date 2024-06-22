using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NPCSpawnCommand : ICommand
{
    private NPCSpawnCommandDef Params;

    public NPCSpawnCommand(NPCSpawnCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}