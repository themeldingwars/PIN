using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireProjectileSlopeCommand : Command, ICommand
{
    private RequireProjectileSlopeCommandDef Params;

    public RequireProjectileSlopeCommand(RequireProjectileSlopeCommandDef par)
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