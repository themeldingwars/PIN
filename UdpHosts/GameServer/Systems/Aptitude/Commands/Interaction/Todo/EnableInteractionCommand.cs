using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class EnableInteractionCommand : Command, ICommand
{
    private EnableInteractionCommandDef Params;

    public EnableInteractionCommand(EnableInteractionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}