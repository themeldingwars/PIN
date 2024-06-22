using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockCertsCommand : ICommand
{
    private UnlockCertsCommandDef Params;

    public UnlockCertsCommand(UnlockCertsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}