using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockVisualOverridesCommand : ICommand
{
    private UnlockVisualOverridesCommandDef Params;

    public UnlockVisualOverridesCommand(UnlockVisualOverridesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}