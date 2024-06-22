using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class HealDamageCommand : ICommand
{
    private HealDamageCommandDef Params;

    public HealDamageCommand(HealDamageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}