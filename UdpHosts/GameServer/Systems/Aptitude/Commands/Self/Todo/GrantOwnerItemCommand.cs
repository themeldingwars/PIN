using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

public class GrantOwnerItemCommand : Command, ICommand
{
    private GrantOwnerItemCommandDef Params;

    public GrantOwnerItemCommand(GrantOwnerItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}