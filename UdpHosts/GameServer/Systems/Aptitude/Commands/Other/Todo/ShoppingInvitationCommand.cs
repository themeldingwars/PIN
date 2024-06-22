using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ShoppingInvitationCommand : ICommand
{
    private ShoppingInvitationCommandDef Params;

    public ShoppingInvitationCommand(ShoppingInvitationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}