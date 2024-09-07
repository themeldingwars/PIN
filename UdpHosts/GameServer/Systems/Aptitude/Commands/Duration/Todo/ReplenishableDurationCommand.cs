using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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