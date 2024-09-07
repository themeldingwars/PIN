using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockOrnamentsCommand : Command, ICommand
{
    private UnlockOrnamentsCommandDef Params;

    public UnlockOrnamentsCommand(UnlockOrnamentsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}