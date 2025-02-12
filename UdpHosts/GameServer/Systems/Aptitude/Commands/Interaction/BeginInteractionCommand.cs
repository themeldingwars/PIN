using System;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class BeginInteractionCommand : ICommand
{
    public BeginInteractionCommand(uint id)
    {
        Id = id;
    }

    public uint Id { get; set; } 

    public bool Execute(Context context)
    {
        if (context.Targets.Count == 0)
        {
            return false;
        }

        var interactionEntity = context.Targets.Peek();
        var abilityId = ((BaseEntity)interactionEntity).Interaction.StartedAbilityId;
        if (abilityId != 0)
        {
            var actingEntity = context.Self;
            context.Shard.Abilities.HandleActivateAbility(
                context.Shard,
                interactionEntity,
                abilityId,
                context.Shard.CurrentTime,
                new AptitudeTargets(actingEntity));
        }

        return true;
    }
}