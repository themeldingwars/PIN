using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetPreviousCommand : Command, ICommand
{
    private TargetPreviousCommandDef Params;

    public TargetPreviousCommand(TargetPreviousCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // TODO: Validate if context.Targets is cleared or if FormerTargets are simply appended.
        foreach (var target in context.FormerTargets)
        {
            context.Targets.Push(target);
        }

        if (Params.Clearformer == 1)
        {
            context.FormerTargets.Clear();
        }

        result.SetPass(StatusCode.None);
    }

    public override void Reset(Context context)
    {
        return;
    }
}