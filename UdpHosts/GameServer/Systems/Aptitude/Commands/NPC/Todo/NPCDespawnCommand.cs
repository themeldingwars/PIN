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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}