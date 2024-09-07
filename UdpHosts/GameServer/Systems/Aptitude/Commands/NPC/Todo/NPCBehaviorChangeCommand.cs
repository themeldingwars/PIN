using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NPCBehaviorChangeCommand : Command, ICommand
{
    private NPCBehaviorChangeCommandDef Params;

    public NPCBehaviorChangeCommand(NPCBehaviorChangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}