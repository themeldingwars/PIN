using AeroMessages.GSS.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Vehicle;
using GameServer.Protocol;
using GameServer.Systems.Encounters;
using Serilog;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class EndInteractionCommand : ICommand
{
    protected readonly ILogger Logger = Log.ForContext<BeginInteractionCommand>();
    private const ulong _releaseToleranceMs = 200;

    public EndInteractionCommand(uint id)
    {
        Id = id;
    }

    public uint Id { get; set; }

    public void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity character || character.InteractionTarget == null)
        {
            Logger.Warning("{Command} {CommandId} Called with bad state. Target Count: {TargetCount}, Self {Self}, InteractionTarget {InteractionTarget}",  nameof(EndInteractionCommand), Id, context.Targets.Count, context.Self, ((CharacterEntity)context.Self).InteractionTarget);
            result.SetFail(StatusCode.PINError);
            return;
        }

        var source = (CharacterEntity)context.Self;
        var interactionEntity = (BaseEntity)source.InteractionTarget;
        var elapsedMs = context.Shard.CurrentTimeLong - source.InteractionStartedAt;
        source.SetInteractionTarget(null);

        // Ended before the interaction took as long as its target asks for: cancelled, not completed.
        // The client times the hold itself, so a release right at the end may arrive a little early.
        if (elapsedMs + _releaseToleranceMs < interactionEntity.Interaction.DurationMs)
        {
            result.SetPass();
            return;
        }

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

        if (interactionEntity.Interaction.LeaderboardId != 0)
        {
            character.OpenLeaderboard(interactionEntity.Interaction.LeaderboardId);
        }

        var abilityId = interactionEntity.Interaction.CompletedAbilityId;
        if (abilityId != 0)
        {
                context.Shard.Abilities.HandleActivateAbility(
                    context.Shard,
                    (IAptitudeTarget)interactionEntity,
                    abilityId,
                    context.Shard.CurrentTime,
                    new AptitudeTargets(character),
                    context.ExecutionId);
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

        result.SetPass();
        return;
    }
}