using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

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