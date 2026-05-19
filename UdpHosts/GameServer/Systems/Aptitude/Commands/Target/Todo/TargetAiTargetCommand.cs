using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetAiTargetCommand : Command, ICommand
{
    private TargetAiTargetCommandDef Params;

    public TargetAiTargetCommand(TargetAiTargetCommandDef par)
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