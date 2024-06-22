using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireBackstabCommand : ICommand
{
    private RequireBackstabCommandDef Params;

    public RequireBackstabCommand(RequireBackstabCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}