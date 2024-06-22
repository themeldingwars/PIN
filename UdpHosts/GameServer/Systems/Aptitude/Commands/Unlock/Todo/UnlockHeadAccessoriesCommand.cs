using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockHeadAccessoriesCommand : ICommand
{
    private UnlockHeadAccessoriesCommandDef Params;

    public UnlockHeadAccessoriesCommand(UnlockHeadAccessoriesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}