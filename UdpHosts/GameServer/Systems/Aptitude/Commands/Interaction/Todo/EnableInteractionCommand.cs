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

    public bool Execute(Context context)
    {
        return true;
    }
}