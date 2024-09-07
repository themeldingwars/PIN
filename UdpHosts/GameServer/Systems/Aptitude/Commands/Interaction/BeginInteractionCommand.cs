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
        if (context.Targets.Count > 0)
        {
            var interactionEntity = context.Targets.Peek();
            var hack = interactionEntity as BaseEntity;
            var abilityId = hack.Interaction.StartedAbilityId;
            if (abilityId != 0)
            {
                var actingEntity = context.Self;
                context.Shard.Abilities.HandleActivateAbility(context.Shard, actingEntity, abilityId, context.Shard.CurrentTime, new AptitudeTargets(interactionEntity));
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}