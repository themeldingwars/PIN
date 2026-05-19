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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}