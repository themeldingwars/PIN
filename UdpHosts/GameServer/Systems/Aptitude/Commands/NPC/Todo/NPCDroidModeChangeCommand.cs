using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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