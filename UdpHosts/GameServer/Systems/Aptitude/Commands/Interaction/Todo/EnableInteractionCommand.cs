using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EnableInteractionCommand : ICommand
{
    private EnableInteractionCommandDef Params;

    public EnableInteractionCommand(EnableInteractionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}