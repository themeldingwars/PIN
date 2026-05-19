using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

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