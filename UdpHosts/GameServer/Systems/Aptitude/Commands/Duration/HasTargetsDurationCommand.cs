using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class HasTargetsDurationCommand : Command, ICommand
{
    private HasTargetsDurationCommandDef Params;

    public HasTargetsDurationCommand(HasTargetsDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.PreviousResult.RawValue == 0x00060003)
        {
            // This is implemented in the game client, idk
            // wait_pass_status6.
            Logger.Warning("Special HasTargetsDuration condition triggered, automatically passing");
            result.SetPass();
            return;
        }

        bool failsTargets = context.Targets.Count == 0 || context.Targets.Count < Params.MinCount;
        bool negate = Params.Negate == 1;

        if (failsTargets == negate)
        {
            result.SetPass(StatusCode.Status3_TargetingFail);
        }
        else
        {
            result.SetFail(StatusCode.Status3_TargetingFail);
        }

        return;
    }
}