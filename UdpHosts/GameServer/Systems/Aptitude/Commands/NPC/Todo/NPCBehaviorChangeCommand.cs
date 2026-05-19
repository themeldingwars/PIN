using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

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