using AeroMessages.GSS.V66.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Vehicle;
using GameServer.Systems.Encounters;

namespace GameServer.Aptitude;

public class EndInteractionCommand : ICommand
{
    public EndInteractionCommand(uint id)
    {
        Id = id;
    }

    public uint Id { get; set; } 

    public bool Execute(Context context)
    {
        if (context.Targets.Count == 0 || context.Self is not CharacterEntity character)
        {
            return false;
        }

        var interactionEntity = (BaseEntity)context.Targets.Peek();

        if (character is { IsPlayerControlled: true })
        {
            var message = new InteractionCompleted { Percent = 100 };
            character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }

        if (interactionEntity.Encounter is { Instance: IInteractionHandler encounter })
        {
            encounter.OnInteraction(character, interactionEntity);
        }

        if (interactionEntity.Encounter is { SpawnDef: { } spawnData })
        {
            context.Shard.EncounterMan.Factory.SpawnEncounter(spawnData, character);
        }

        var abilityId = interactionEntity.Interaction.CompletedAbilityId;
        if (abilityId != 0)
        {
                context.Shard.Abilities.HandleActivateAbility(
                    context.Shard,
                    (IAptitudeTarget)interactionEntity,
                    abilityId,
                    context.Shard.CurrentTime,
                    new AptitudeTargets(character));
        }

        var interactionType = interactionEntity.Interaction.Type;

            // if (hack is DeployableEntity { Turret: not null } deployable)
            // {
            //     var character = initiator as CharacterEntity;
            //
            //     deployable.Turret.SetControllingPlayer(character.Player);
            // }
        if (interactionType == InteractionType.Vehicle && interactionEntity is VehicleEntity vehicle)
        {
            vehicle.AddOccupant(character);
        }

        return true;
    }
}