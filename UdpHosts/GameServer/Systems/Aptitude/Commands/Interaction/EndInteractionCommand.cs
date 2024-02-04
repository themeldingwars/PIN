using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.V66.Character;
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
            if (actingEntity.GetType() == typeof(Entities.Character.CharacterEntity))
            {
                var character = actingEntity as Entities.Character.CharacterEntity;
                
                if (character.IsPlayerControlled)
                {
                    var message = new InteractionCompleted { Percent = 100 };
                    character.Player.NetChannels[ChannelType.ReliableGss].SendIAero(message, character.EntityId);
                }
            }

            var interactionEntity = context.Targets.First();
            var hack = interactionEntity as BaseEntity;
            var abilityId = hack.Interaction.CompletedAbilityId;
            if (abilityId != 0)
            {
                context.Shard.Abilities.HandleActivateAbility(context.Shard, actingEntity, abilityId, context.Shard.CurrentTime, new HashSet<IAptitudeTarget>() { interactionEntity });
            }
            
            var interactionType = hack.Interaction.Type;
            if (interactionType == InteractionType.Vehicle)
            {
                if (actingEntity.GetType() == typeof(Entities.Character.CharacterEntity) && interactionEntity.GetType() == typeof(Entities.Vehicle.VehicleEntity))
                {
                    var character = actingEntity as Entities.Character.CharacterEntity;
                    var vehicle = interactionEntity as Entities.Vehicle.VehicleEntity;

                    var occupiedSeat = vehicle.AddOccupant(character);
                    if (occupiedSeat != null)
                    {
                        character.SetAttachedTo(new AttachedToData()
                        {
                            Id1 = vehicle.AeroEntityId,
                            Id2 = vehicle.AeroEntityId,
                            Role = (AttachedToData.AttachmentRoleType)occupiedSeat.Role,
                            Unk2 = 3, // posture?
                            Unk3 = 1,
                        }, vehicle);

                        // Force scope in of vehicle so that controllers are sent down
                        if (character.IsPlayerControlled && occupiedSeat.Role == Entities.Vehicle.AttachmentRole.Driver)
                        {
                            var player = character.Player;
                            var entityMan = player.AssignedShard.EntityMan;
                            entityMan.ScopeIn(player, vehicle);
                        }
                    }
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}