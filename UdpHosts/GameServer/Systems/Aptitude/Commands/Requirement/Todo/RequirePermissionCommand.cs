using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequirePermissionCommand : Command, ICommand
{
    private RequirePermissionCommandDef Params;

    public RequirePermissionCommand(RequirePermissionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}