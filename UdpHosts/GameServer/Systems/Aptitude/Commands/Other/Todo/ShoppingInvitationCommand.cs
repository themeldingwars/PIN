using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ShoppingInvitationCommand : Command, ICommand
{
    private ShoppingInvitationCommandDef Params;

    public ShoppingInvitationCommand(ShoppingInvitationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}