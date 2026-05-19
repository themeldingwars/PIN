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

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}