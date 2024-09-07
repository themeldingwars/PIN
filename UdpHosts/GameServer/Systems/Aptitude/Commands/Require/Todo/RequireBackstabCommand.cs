using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireBackstabCommand : Command, ICommand
{
    private RequireBackstabCommandDef Params;

    public RequireBackstabCommand(RequireBackstabCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}