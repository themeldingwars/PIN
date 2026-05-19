using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Duration;

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