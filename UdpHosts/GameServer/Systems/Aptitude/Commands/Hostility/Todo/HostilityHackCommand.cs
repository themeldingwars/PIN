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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}