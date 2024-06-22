using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockOrnamentsCommand : ICommand
{
    private UnlockOrnamentsCommandDef Params;

    public UnlockOrnamentsCommand(UnlockOrnamentsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}