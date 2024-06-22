using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class GrantOwnerItemCommand : ICommand
{
    private GrantOwnerItemCommandDef Params;

    public GrantOwnerItemCommand(GrantOwnerItemCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}