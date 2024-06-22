using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class CombatFlagsCommand : ICommand
{
    private CombatFlagsCommandDef Params;

    public CombatFlagsCommand(CombatFlagsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}