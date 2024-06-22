using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class LifespanDurationCommand : ICommand
{
    private LifespanDurationCommandDef Params;

    public LifespanDurationCommand(LifespanDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}