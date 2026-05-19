using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Object;

public class SetOrientationCommand : Command, ICommand
{
    private SetOrientationCommandDef Params;

    public SetOrientationCommand(SetOrientationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}