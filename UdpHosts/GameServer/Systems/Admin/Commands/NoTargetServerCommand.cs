using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Toggle invisibility to AI targeting", "notarget", "notarget", "ai_invisible")]
public class NoTargetServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot toggle notarget without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is CharacterEntity commandTarget)
        {
            character = commandTarget;
        }

        character.NotTargetable = !character.NotTargetable;

        SourceFeedback($"NotTarget {(character.NotTargetable ? "ON" : "OFF")}", context);
    }
}
