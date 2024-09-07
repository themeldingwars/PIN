using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EnableInteractionCommand : Command, ICommand
{
    private EnableInteractionCommandDef Params;

    public EnableInteractionCommand(EnableInteractionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}