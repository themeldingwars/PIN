using AeroMessages.GSS.V66.Character;
using GameServer.Entities.Character;

namespace GameServer.Admin;

[ServerCommand("Perform an emote by id. Only displays on remote views.", "emote <id>", "emote")]
public class EmoteServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Invalid number of parameters for emote command", context);
            return;
        }

        CharacterEntity character;
        if (context.Target != null)
        {
            if (context.Target is CharacterEntity targetCharacter)
            {
                character = targetCharacter;
            }
            else
            {
                SourceFeedback("The target is not a Character", context);
                return;
            }
        }
        else
        {
            if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
            {
                SourceFeedback("Cannot emote without a valid player character", context);
                return;
            }

            character = context.SourcePlayer.CharacterEntity;
        }

        ushort emoteId = (ushort)ParseUIntParameter(parameters[0]);
        uint time = context.Shard.CurrentTime;

        character.SetEmote(new EmoteData { Id = emoteId, Time = time + 60 });

        SourceFeedback($"Set emote {emoteId}", context);
    }
}