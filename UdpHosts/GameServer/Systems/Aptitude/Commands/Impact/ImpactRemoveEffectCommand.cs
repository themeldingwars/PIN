using System.Linq;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ImpactRemoveEffectCommand : Command, ICommand
{
    private ImpactRemoveEffectCommandDef Params;

    public ImpactRemoveEffectCommand(ImpactRemoveEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.EffectId != null)
        {
            uint effectId = (uint)Params.EffectId;
            if (Params.RemoveFromSelf != null && Params.RemoveFromSelf == true)
            {
                context.Abilities.DoRemoveEffect(context.Self, effectId);
            }
            else
            {
                foreach (IAptitudeTarget target in context.Targets)
                {
                   context.Abilities.DoRemoveEffect(target, effectId);
                }
            }
        }
        else
        {
            var activefx = context.Self.GetActiveEffects();
            string effectIds = string.Join(", ", activefx.Where(ae => ae?.Effect != null).Select(ae => ae.Effect.Id));
            Logger.Warning("Active Effects (Self): {Message}", effectIds);
            Logger.Warning("Don't know which effect to remove for {Command} {CommandId}", nameof(ImpactRemoveEffectCommand), Params.Id);
        }

        return true;
    }
}