using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class LifespanDurationCommand : Command, ICommand
{
    private LifespanDurationCommandDef Params;

    public LifespanDurationCommand(LifespanDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}