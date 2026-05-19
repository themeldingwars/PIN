using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Hostility;

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