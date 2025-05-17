using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddAccountGroupCommand : Command, ICommand
{
    private AddAccountGroupCommandDef Params;

    public AddAccountGroupCommand(AddAccountGroupCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // examples:
        // vip
        // mamba lgv rental (41157)
        return true;
    }
}