using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireResourceCommand : ICommand
{
    private RequireResourceCommandDef Params;

    public RequireResourceCommand(RequireResourceCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}