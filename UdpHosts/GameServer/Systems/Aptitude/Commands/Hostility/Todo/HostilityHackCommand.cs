using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class HostilityHackCommand : ICommand
{
    private HostilityHackCommandDef Params;

    public HostilityHackCommand(HostilityHackCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}