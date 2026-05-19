using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class TeleportCommand : Command, ICommand
{
    private TeleportCommandDef Params;

    public TeleportCommand(TeleportCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}