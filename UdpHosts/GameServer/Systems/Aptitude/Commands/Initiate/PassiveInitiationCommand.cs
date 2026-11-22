using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Initiate;

public class PassiveInitiationCommand : Command, ICommand
{
    private const uint IdleRecheckMs = 60000;

    private PassiveInitiationCommandDef Params;

    public PassiveInitiationCommand(PassiveInitiationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.InitiationInterval != 0)
        {
            context.InitTime ??= 0;
            if (!ProcessSchedule.Due(context, Params.InitiationInterval, context.AbilityId))
            {
                result.SetPass();
                result.SetYield();
                return;
            }

            context.InitTime = context.CurrentTime;
            context.SnapshotInitPose();
            result.SetPass();
            return;
        }

        if (context.InitTime == null)
        {
            context.InitTime = context.CurrentTime;
            context.SnapshotInitPose();
            result.SetPass();
            return;
        }

        context.NextProcessTime = unchecked(context.CurrentTime + IdleRecheckMs);
        result.SetPass();
        result.SetYield();
    }
}
