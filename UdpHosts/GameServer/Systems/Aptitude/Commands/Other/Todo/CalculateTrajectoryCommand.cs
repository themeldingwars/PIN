using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CalculateTrajectoryCommand : ICommand
{
    private CalculateTrajectoryCommandDef Params;

    public CalculateTrajectoryCommand(CalculateTrajectoryCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}