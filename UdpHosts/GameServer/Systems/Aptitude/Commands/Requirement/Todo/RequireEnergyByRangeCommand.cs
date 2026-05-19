using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireEnergyByRangeCommand : Command, ICommand
{
    private RequireEnergyByRangeCommandDef Params;

    public RequireEnergyByRangeCommand(RequireEnergyByRangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}