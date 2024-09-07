using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockVisualOverridesCommand : Command, ICommand
{
    private UnlockVisualOverridesCommandDef Params;

    public UnlockVisualOverridesCommand(UnlockVisualOverridesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}