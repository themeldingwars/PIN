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

    public bool Execute(Context context)
    {
        return true;
    }
}