using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NPCBehaviorChangeCommand : ICommand
{
    private NPCBehaviorChangeCommandDef Params;

    public NPCBehaviorChangeCommand(NPCBehaviorChangeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}