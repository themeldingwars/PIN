using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireEliteLevelCommand : Command, ICommand
{
    private RequireEliteLevelCommandDef Params;

    public RequireEliteLevelCommand(RequireEliteLevelCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}