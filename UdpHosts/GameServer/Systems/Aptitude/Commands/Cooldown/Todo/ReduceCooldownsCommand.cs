using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Cooldown;

public class ReduceCooldownsCommand : Command, ICommand
{
    private ReduceCooldownsCommandDef Params;

    public ReduceCooldownsCommand(ReduceCooldownsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}