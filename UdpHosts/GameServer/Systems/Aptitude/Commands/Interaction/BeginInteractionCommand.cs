using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Data.SDB.Records.apt;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class BeginInteractionCommand : ICommand
{
    public BeginInteractionCommand()
    {
    }

    public bool Execute(Context context)
    {
        if (context.Targets.Count > 0)
        {
            var interactionEntity = context.Targets.First();
            var hack = interactionEntity as BaseEntity;
            var abilityId = hack.Interaction.StartedAbilityId;
            if (abilityId != 0)
            {
                var actingEntity = context.Self;
                context.Shard.Abilities.HandleActivateAbility(context.Shard, actingEntity, abilityId, context.Shard.CurrentTime, new HashSet<IAptitudeTarget>() { interactionEntity });
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}