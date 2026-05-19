using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class TimeCooldownCommand : Command, ICommand
{
    private TimeCooldownCommandDef Params;

    public TimeCooldownCommand(TimeCooldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}