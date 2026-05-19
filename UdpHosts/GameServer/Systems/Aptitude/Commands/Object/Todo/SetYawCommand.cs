using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Object;

public class SetYawCommand : Command, ICommand
{
    private SetYawCommandDef Params;

    public SetYawCommand(SetYawCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}