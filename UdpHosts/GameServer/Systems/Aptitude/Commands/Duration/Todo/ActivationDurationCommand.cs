using System.Collections.Generic;
using System.Linq;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class ActivationDurationCommand : Command, ICommand
{
    private ActivationDurationCommandDef Params;

    public ActivationDurationCommand(ActivationDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    /*
    * We assume that Activation refers to abilities that have a time to activate (are active while button is held), and mostly think of Interactions.
    * We assume that since this is a Duration command it should be true if we have not yet finished the Activation / Interaction. It should return false when the Activation / Interaction is completed.
    * The params AbilityId and Activated exist. So we assume we must check for the Activation of a specific AbilityId. We assume Activated means true/false based on whether it is activated.
    */
    public bool Execute(Context context)
    {
        // We determine activated ability from context, hope we passed that everywhere it was needed...
        // TODO: Maybe we should store active abilities on entity and check against that instead of expecting it from context
        var isAbilityActivated = context.AbilityId == Params.AbilityId;

        // Command wants to see if ability is activated and we have activated it
        if (Params.Activated == 1 && isAbilityActivated)
        {
            // We don't know how to tract activation duration. We assume it is interaction duration.
            // We assume we must check against the target.
            // We assume we can use the init time as comparison.
            if (context.Targets.Count > 0)
            {
                var interactionEntity = context.Targets.Peek();
                var hack = interactionEntity as BaseEntity;
                var durationMs = hack.Interaction.DurationMs;
                var currentMs = context.Shard.CurrentTime;
                var initMs = context.InitTime;

                return currentMs < initMs + durationMs;
            }

            // If we have no targets we assume we should fail
            else
            {
                return false;
            }
        }

        // Command does not want activation and we have not activated it
        else if (Params.Activated == 0 && !isAbilityActivated)
        {
            return true;
        }

        // Missmatch, either we didn't have this ability activated or we shouldn't have had it
        else 
        {
            return false;
        }
    }
}