using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

public class NPCDroidModeChangeCommand : Command, ICommand
{
    private NPCDroidModeChangeCommandDef Params;

    public NPCDroidModeChangeCommand(NPCDroidModeChangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}