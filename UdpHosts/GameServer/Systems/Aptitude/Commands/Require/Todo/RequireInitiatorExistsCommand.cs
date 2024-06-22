using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequireInitiatorExistsCommand : ICommand
{
    private RequireInitiatorExistsCommandDef Params;

    public RequireInitiatorExistsCommand(RequireInitiatorExistsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}