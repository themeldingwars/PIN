using AeroMessages.GSS.Character.Controller;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Authorize a terminal for your character", "dbg_terminal <type> <id> [entity]", "dbg_terminal")]
public class DebugAuthTerminalServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length is < 2 or > 3)
        {
            SourceFeedback("Invalid number of parameters for debug auth terminal command", context);
            return;
        }

        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot debug auth terminal without a valid player character", context);
            return;
        }

        uint type = ParseUIntParameter(parameters[0]);
        uint id = ParseUIntParameter(parameters[1]);

        var character = context.SourcePlayer.CharacterEntity;

        // The terminal entity defaults to the player's own character, since the client ignores an entity id it doesn't know
        var entity = parameters.Length == 3 && ulong.TryParse(parameters[2], out var parsed) ? parsed : character.AeroEntityId.Backing;

        character.SetAuthorizedTerminal(new AuthorizedTerminalData
        {
            TerminalType = (byte)type,
            TerminalId = id,
            TerminalEntityId = entity
        });
    }
}