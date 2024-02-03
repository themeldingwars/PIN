using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB.Records.apt;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class EndInteractionCommand : ICommand
{
    public EndInteractionCommand()
    {
    }

    public bool Execute(Context context)
    {
        if (context.Targets.Count > 0)
        {
            var actingEntity = context.Self;
            if (actingEntity.GetType() == typeof(Entities.Character.Character))
            {
                var character = actingEntity as Entities.Character.Character;
                
                if (character.IsPlayerControlled)
                {
                    var message = new InteractionCompleted { Percent = 100 };
                    character.Player.NetChannels[ChannelType.ReliableGss].SendIAero(message, character.EntityId);
                }
            }

            var interactionEntity = context.Targets.First();
            var hack = interactionEntity as BaseEntity;
            var abilityId = hack.Interaction.CompletedAbilityId;
            context.Shard.Abilities.HandleActivateAbility(context.Shard, actingEntity, abilityId, context.Shard.CurrentTime, new HashSet<IAptitudeTarget>() { interactionEntity });
            return true;
        }
        else
        {
            return false;
        }
    }
}