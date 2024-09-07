using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockCertsCommand : Command, ICommand
{
    private UnlockCertsCommandDef Params;

    public UnlockCertsCommand(UnlockCertsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}