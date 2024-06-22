using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequirePermissionCommand : ICommand
{
    private RequirePermissionCommandDef Params;

    public RequirePermissionCommand(RequirePermissionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}