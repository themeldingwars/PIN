using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TimedActivationCommand : ICommand
{
    private TimedActivationCommandDef Params;

    public TimedActivationCommand(TimedActivationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}