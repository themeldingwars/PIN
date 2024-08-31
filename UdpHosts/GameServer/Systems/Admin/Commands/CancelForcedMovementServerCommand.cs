using AeroMessages.GSS.V66.Character.Event;

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
        
        uint commandId = ParseUIntParameter(parameters[0]);
        var character = context.SourcePlayer.CharacterEntity;
        var player = character.Player;
        var message = new ForcedMovementCancelled
        {
            CommandId = commandId,
            ShortTime = context.Shard.CurrentShortTime,
        };
        player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);

        SourceFeedback($"Cancelling forced movement {commandId}", context);
    }
}
