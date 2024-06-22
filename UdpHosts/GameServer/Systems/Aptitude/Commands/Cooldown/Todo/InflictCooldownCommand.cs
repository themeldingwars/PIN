using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class InflictCooldownCommand : ICommand
{
    private InflictCooldownCommandDef Params;

    public InflictCooldownCommand(InflictCooldownCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}