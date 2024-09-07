using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReplenishEffectDurationCommand : Command, ICommand
{
    private ReplenishEffectDurationCommandDef Params;

    public ReplenishEffectDurationCommand(ReplenishEffectDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}