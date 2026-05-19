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

    public override void Execute(Context context, ref CommandResult result)
    {
        // examples:
        // vip
        // mamba lgv rental (41157)
        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}