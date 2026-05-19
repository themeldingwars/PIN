using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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