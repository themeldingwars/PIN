using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReplenishableDurationCommand : ICommand
{
    private ReplenishableDurationCommandDef Params;

    public ReplenishableDurationCommand(ReplenishableDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}