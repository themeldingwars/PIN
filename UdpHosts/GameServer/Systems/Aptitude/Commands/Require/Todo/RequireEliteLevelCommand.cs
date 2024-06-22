using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireEliteLevelCommand : ICommand
{
    private RequireEliteLevelCommandDef Params;

    public RequireEliteLevelCommand(RequireEliteLevelCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}