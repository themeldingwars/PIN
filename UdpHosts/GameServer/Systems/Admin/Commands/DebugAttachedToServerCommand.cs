using AeroMessages.GSS.V66.Character;
using static AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData;

namespace GameServer.Admin;

[ServerCommand("Debug AttachedTo", "dbgattach <unk2> <unk3>", "dbgattach")]
public class DebugAttachedToServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;

        if (character.AttachedTo == null)
        {
            SourceFeedback("Character must be attached", context);
            return;
        }

        if (parameters.Length != 2)
        {
            SourceFeedback("Bad params", context);
            return;
        }

        byte value1 = (byte)ParseUIntParameter(parameters[0]);
        byte value2 = (byte)ParseUIntParameter(parameters[1]);

        var prevData = (AttachedToData)character.AttachedTo;
        character.SetAttachedTo(new AttachedToData
        {
            Id1 = prevData.Id1,
            Id2 = prevData.Id2,
            Role = prevData.Role,
            Unk2 = value1,
            Unk3 = value2,
        }, character.AttachedToEntity);
        SourceFeedback($"Setting Unk2 = {value1}, Unk3 = {value2} (Role {prevData.Role})", context);
    }
}
