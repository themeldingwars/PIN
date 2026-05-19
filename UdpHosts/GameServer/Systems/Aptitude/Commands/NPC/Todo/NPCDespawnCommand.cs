using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

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