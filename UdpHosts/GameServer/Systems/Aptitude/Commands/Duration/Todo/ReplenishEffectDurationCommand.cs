using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReplenishEffectDurationCommand : ICommand
{
    private ReplenishEffectDurationCommandDef Params;

    public ReplenishEffectDurationCommand(ReplenishEffectDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}