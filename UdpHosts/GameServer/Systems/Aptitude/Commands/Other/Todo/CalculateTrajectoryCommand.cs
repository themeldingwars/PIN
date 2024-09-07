using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CalculateTrajectoryCommand : Command, ICommand
{
    private CalculateTrajectoryCommandDef Params;

    public CalculateTrajectoryCommand(CalculateTrajectoryCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}