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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}