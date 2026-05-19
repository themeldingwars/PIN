using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

public class NPCEquipMonsterCommand : Command, ICommand
{
    private NPCEquipMonsterCommandDef Params;

    public NPCEquipMonsterCommand(NPCEquipMonsterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}