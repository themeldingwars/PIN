using GameServer.Entities;
using GameServer.Entities.Character;
using Serilog;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class BeginInteractionCommand : ICommand
{
    protected readonly ILogger Logger = Log.ForContext<BeginInteractionCommand>();

    public BeginInteractionCommand(uint id)
    {
        Id = id;
    }

    public uint Id { get; set; }

    public void Execute(Context context, ref CommandResult result)
    {
        if (context.Targets.Count == 0 || context.Self is not CharacterEntity character)
        {
            Logger.Warning("{Command} {CommandId} Called with bad state. Target Count: {TargetCount}, Self {Self}",  nameof(BeginInteractionCommand), Id, context.Targets.Count, context.Self);
            result.SetFail(StatusCode.PINError);
            return;
        }

        var source = (CharacterEntity)context.Self;
        var target = context.Targets.Peek();
        source.SetInteractionTarget((IEntity)target);

        var abilityId = ((BaseEntity)target).Interaction.StartedAbilityId;
        if (abilityId != 0)
        {
            context.Shard.Abilities.HandleActivateAbility(
                context.Shard,
                target,
                abilityId,
                context.Shard.CurrentTime,
                new AptitudeTargets(source),
                context.ExecutionId);
        }

        result.SetPass();
        return;
    }
}