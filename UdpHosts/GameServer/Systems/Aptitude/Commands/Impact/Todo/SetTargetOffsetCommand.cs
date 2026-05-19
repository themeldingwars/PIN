using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class SetTargetOffsetCommand : Command, ICommand
{
    private SetTargetOffsetCommandDef Params;

    public SetTargetOffsetCommand(SetTargetOffsetCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}