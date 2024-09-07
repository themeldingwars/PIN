using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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