using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetBySinVulnerableCommand : Command, ICommand
{
    private TargetBySinVulnerableCommandDef Params;

    public TargetBySinVulnerableCommand(TargetBySinVulnerableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}