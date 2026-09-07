using GameServer.Extensions;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Impact;

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
                return context.Abilities.DoRemoveEffect(context.Self, effectId);
            }

            foreach (IAptitudeTarget target in context.Targets)
            {
                if (!context.Abilities.DoRemoveEffect(target, effectId))
                {
                    return false;
                }
            }
        }
        else
        {
            // aptgss::ImpactRemoveEffectCommandDef rows without an effect id ask for the effects of *other*
            // abilities to be cleared (a boost pad, for example, takes away the launch effect of the other pad
            // flavours before applying its own). The id of the effect to clear is exactly the piece of data the
            // definition we have does not carry, so there is nothing we may remove: guessing "the effects of the
            // ability that is running right now" would undo the ImpactApplyEffect earlier in the same chain,
            // which takes the glider permission and the launch state away again the moment they are granted.
            // Report it once per command id instead of twice per tick: this chain belongs to abilities that are
            // re-triggered by the client while the player stays in range, and the whole apply/remove/re-trigger
            // cycle runs at the update rate of the shard.
            if (OnceLog.ShouldLog((nameof(ImpactRemoveEffectCommand), Params.Id)))
            {
                Logger.Debug("[{Command} {CommandId}] has no effect id to remove, ignoring", nameof(ImpactRemoveEffectCommand), Params.Id);
            }
        }

        return true;
    }
}