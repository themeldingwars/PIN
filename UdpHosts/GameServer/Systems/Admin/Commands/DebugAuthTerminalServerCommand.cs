using AeroMessages.GSS.Character.Controller;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Create a tiny object", "dbg_terminal <type> <id>", "dbg_terminal")]
public class DebugAuthTerminalServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 2)
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

        character.SetAuthorizedTerminal(new AuthorizedTerminalData
        {
            TerminalType = (byte)type,
            TerminalId = (byte)id,
            TerminalEntityId = character.AeroEntityId.Backing
        });
    }
}