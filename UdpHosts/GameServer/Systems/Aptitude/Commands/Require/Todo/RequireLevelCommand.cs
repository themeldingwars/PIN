using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireLevelCommand : ICommand
{
    private RequireLevelCommandDef Params;

    public RequireLevelCommand(RequireLevelCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}