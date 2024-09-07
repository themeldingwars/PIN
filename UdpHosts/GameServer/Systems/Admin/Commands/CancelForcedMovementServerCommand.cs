using AeroMessages.GSS.V66.Character.Event;
using GameServer.Entities.Character;

namespace GameServer.Admin;

[ServerCommand("Cancel ForcedMovement", "cancelfm <commandId>", "cancelfm")]
public class CancelForcedMovementServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot cancel forced movement without a valid player character", context);
            return;
        }

        if (parameters.Length != 1)
        {
            SourceFeedback("Invalid arguments", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is CharacterEntity commandTarget)
        {
            character = commandTarget;
        }

        if (!character.IsPlayerControlled)
        {
            SourceFeedback("Can only cancel for player controlled characters", context);
            return;
        }

        var player = character.Player;
        uint commandId = ParseUIntParameter(parameters[0]);
        var message = new ForcedMovementCancelled
        {
            CommandId = commandId,
            ShortTime = context.Shard.CurrentShortTime,
        };
        player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);

        SourceFeedback($"Cancelling forced movement {commandId}", context);
    }
}
