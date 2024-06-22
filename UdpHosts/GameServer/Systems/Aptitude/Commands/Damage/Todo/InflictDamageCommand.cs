using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class InflictDamageCommand : ICommand
{
    private InflictDamageCommandDef Params;

    public InflictDamageCommand(InflictDamageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}