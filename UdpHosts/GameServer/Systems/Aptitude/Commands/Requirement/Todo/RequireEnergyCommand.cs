using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireEnergyCommand : Command, ICommand
{
    private RequireEnergyCommandDef Params;

    public RequireEnergyCommand(RequireEnergyCommandDef par)
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