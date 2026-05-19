using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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