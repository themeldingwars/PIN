using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Hostility;

public class HostilityOverrideCommand : Command, ICommand
{
    private HostilityOverrideCommandDef Params;

    public HostilityOverrideCommand(HostilityOverrideCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}