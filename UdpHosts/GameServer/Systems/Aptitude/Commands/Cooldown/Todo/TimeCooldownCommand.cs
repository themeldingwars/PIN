using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TimeCooldownCommand : ICommand
{
    private TimeCooldownCommandDef Params;

    public TimeCooldownCommand(TimeCooldownCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}