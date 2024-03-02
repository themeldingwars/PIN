using GameServer.Aptitude;
using GameServer.Data.SDB;

namespace GameServer.Admin;

[ServerCommand("Remove a status effect", "removeeffect <effectId>", "removeeffect", "remove_effect", "apt_remove", "apt_clear", "apt_cancel")]
public class RemoveEffectServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Invalid number of parameters for remove effect command", context);
            return;
        }

        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot remove effect without a valid player character", context);
            return;
        }

        var shard = context.Shard;
        var character = context.SourcePlayer.CharacterEntity;
        uint effectId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetStatusEffectData(effectId) == null)
        {
            SourceFeedback("No effect with this id", context);
            return;
        }

        shard.Abilities.DoRemoveEffect(character, effectId);
    }
}
