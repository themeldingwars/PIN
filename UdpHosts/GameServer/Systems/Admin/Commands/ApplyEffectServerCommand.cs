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

        uint effectId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetStatusEffectData(effectId) == null)
        {
            SourceFeedback("No effect with this id", context);
            return;
        }

        IAptitudeTarget initiator = context.SourcePlayer.CharacterEntity;
        IAptitudeTarget target = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is IAptitudeTarget commandTarget)
        {
            target = commandTarget;
        }

        var shard = context.Shard;
        shard.Abilities.DoApplyEffect(effectId, target, new Context(shard, initiator)
        {
            InitTime = shard.CurrentTime,
        });
    }
}