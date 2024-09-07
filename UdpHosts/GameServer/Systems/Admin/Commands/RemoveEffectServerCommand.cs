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

        uint effectId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetStatusEffectData(effectId) == null)
        {
            SourceFeedback("No effect with this id", context);
            return;
        }

        var shard = context.Shard;
        IAptitudeTarget target = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is IAptitudeTarget commandTarget)
        {
            target = commandTarget;
        }

        shard.Abilities.DoRemoveEffect(target, effectId);
    }
}
