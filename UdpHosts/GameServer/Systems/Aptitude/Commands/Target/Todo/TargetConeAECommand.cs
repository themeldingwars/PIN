using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetConeAECommand : Command, ICommand
{
    private TargetConeAECommandDef Params;

    public TargetConeAECommand(TargetConeAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}