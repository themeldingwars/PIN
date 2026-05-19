using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Activation;

public class TimedActivationCommand : Command, ICommand
{
    private TimedActivationCommandDef Params;

    public TimedActivationCommand(TimedActivationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}