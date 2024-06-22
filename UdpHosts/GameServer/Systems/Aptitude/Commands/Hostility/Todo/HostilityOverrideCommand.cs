using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class HostilityOverrideCommand : ICommand
{
    private HostilityOverrideCommandDef Params;

    public HostilityOverrideCommand(HostilityOverrideCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}