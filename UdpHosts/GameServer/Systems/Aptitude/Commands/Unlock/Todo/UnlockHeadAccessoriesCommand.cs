using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockHeadAccessoriesCommand : Command, ICommand
{
    private UnlockHeadAccessoriesCommandDef Params;

    public UnlockHeadAccessoriesCommand(UnlockHeadAccessoriesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}