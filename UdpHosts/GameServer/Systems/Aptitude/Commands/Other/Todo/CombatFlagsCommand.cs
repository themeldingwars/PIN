using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class CombatFlagsCommand : Command, ICommand
{
    private CombatFlagsCommandDef Params;

    public CombatFlagsCommand(CombatFlagsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}