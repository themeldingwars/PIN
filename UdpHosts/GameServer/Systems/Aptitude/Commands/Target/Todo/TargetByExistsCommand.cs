using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetByExistsCommand : ICommand
{
    private TargetByExistsCommandDef Params;

    public TargetByExistsCommand(TargetByExistsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}