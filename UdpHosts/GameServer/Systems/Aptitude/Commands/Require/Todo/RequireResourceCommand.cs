using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireResourceCommand : Command, ICommand
{
    private RequireResourceCommandDef Params;

    public RequireResourceCommand(RequireResourceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}