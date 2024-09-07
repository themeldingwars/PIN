using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NPCSpawnCommand : Command, ICommand
{
    private NPCSpawnCommandDef Params;

    public NPCSpawnCommand(NPCSpawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}