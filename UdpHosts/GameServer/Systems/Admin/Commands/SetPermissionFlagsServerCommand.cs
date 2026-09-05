using GameServer.Entities.Character;
using GameServer.Systems.Aptitude;
using static AeroMessages.GSS.Character.Controller.PermissionFlagsData;

namespace GameServer.Systems.Admin.Commands;

// TODO: Implement ability to pick flags, for now I just need cheat_float :)
[ServerCommand("Set Character PermissionFlags", "pflags [value]", "pflags", "pflag", "float")]
public class SetPermissionFlagsServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot change permission flags without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is CharacterEntity commandTarget)
        {
            character = commandTarget;
        }

        bool newFloatValue = !character.CurrentPermissions[CharacterPermissionFlags.cheat_float];
        character.SetPermissionFlag(CharacterPermissionFlags.cheat_float, newFloatValue);

        SourceFeedback($"Setting CharacterPermissionFlags.cheat_float to {newFloatValue}", context);

        const uint customRef = 99999991;
        if (newFloatValue)
        {
            character.AddStatModifier(customRef, new ActiveStatModifier { Stat = Enums.AptitudeStat.Gravity, Multi = 0.5f });
        }
        else
        {
            character.RemoveStatModifier(customRef, Enums.AptitudeStat.Gravity);
        }
    }
}