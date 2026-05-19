using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class ApplyImpulseCommand : Command, ICommand
{
    private ApplyImpulseCommandDef Params;

    public ApplyImpulseCommand(ApplyImpulseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}