using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class ReplenishableDurationCommand : Command, ICommand
{
    private ReplenishableDurationCommandDef Params;

    public ReplenishableDurationCommand(ReplenishableDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}