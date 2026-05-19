using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByHostilityCommand : Command, ICommand
{
    private TargetByHostilityCommandDef Params;

    public TargetByHostilityCommand(TargetByHostilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}