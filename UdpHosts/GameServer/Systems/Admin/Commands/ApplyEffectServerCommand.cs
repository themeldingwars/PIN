using GameServer.Aptitude;
using GameServer.Data.SDB;

namespace GameServer.Admin;

[ServerCommand("Apply a status effect", "applyeffect <effectId>", "applyeffect", "apply_effect", "apt_apply")]
public class ApplyEffectServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Invalid number of parameters for apply effect command", context);
            return;
        }

        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot apply effect without a valid player character", context);
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

        shard.Abilities.DoApplyEffect(effectId, character, new Context(shard, character)
        {
            InitTime = shard.CurrentTime,
        });
    }
}
