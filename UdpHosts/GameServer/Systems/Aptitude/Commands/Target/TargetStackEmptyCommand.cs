using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetStackEmptyCommand : Command, ICommand
{
    private TargetStackEmptyCommandDef Params;

    public TargetStackEmptyCommand(TargetStackEmptyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var outcome = new CommandResult() { Success = true };

        var shouldNotBeEmpty = Params.NotEmpty == 1;
        var isEmpty = context.TargetStack.Count == 0;

        if (isEmpty == shouldNotBeEmpty)
        {
            outcome.SetFail(StatusCode.Status3_TargetingFail);
        }

        result = outcome;
    }

    public override void Reset(Context context)
    {
        return;
    }
}