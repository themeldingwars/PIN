using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class StagedActivationCommand : Command, ICommand
{
    private StagedActivationCommandDef Params;

    public StagedActivationCommand(StagedActivationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // PassRegister and PassBonus are not handled, but they're 0 for all instances; AllowPrediction is client-side
        if (Params.SelfEffectId != 0)
        {
            context.Shard.Abilities.DoApplyEffect(Params.SelfEffectId, context.Self, context);
        }

        return true;
    }
}