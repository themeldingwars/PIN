using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetSquadmatesCommand : Command, ICommand
{
    private TargetSquadmatesCommandDef Params;

    public TargetSquadmatesCommand(TargetSquadmatesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}