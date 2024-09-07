using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class HostilityHackCommand : Command, ICommand
{
    private HostilityHackCommandDef Params;

    public HostilityHackCommand(HostilityHackCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}