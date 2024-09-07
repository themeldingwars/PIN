using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ClearHostilityCommand : Command, ICommand
{
    private ClearHostilityCommandDef Params;

    public ClearHostilityCommand(ClearHostilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}