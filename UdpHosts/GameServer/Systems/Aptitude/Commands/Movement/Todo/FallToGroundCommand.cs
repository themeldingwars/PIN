using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class FallToGroundCommand : Command, ICommand
{
    private FallToGroundCommandDef Params;

    public FallToGroundCommand(FallToGroundCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}