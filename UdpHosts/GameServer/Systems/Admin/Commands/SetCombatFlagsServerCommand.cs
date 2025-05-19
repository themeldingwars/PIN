using AeroMessages.GSS.V66.Character;
using GameServer.Entities.Character;

namespace GameServer.Admin;

[ServerCommand("Set Character CombatFlags", "cflags [value]", "cflags", "cflag")]
public class SetCombatFlagsServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot change combat flags without a valid player character", context);
            return;
        }

        uint flags = 0;

        if (parameters.Length == 1)
        {
            flags = ParseUIntParameter(parameters[0]);
        }

        var character = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is CharacterEntity commandTarget)
        {
            character = commandTarget;
        }

        var value = new CombatFlagsData { Value = (CombatFlagsData.CharacterCombatFlags)flags, Time = context.Shard.CurrentTime };

        character.SetCombatFlags(value);
        
        SourceFeedback($"Setting {character} combat flags to {flags}", context);
    }
}