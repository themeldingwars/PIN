using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Set your HKM supercharge gauge, without a percent it fills or empties the gauge", "supercharge [percent 0-100]", "supercharge", "hkm")]
public class SuperChargeServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot set the HKM gauge without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        var current = context.Shard.SuperCharge.GetCharge(character);

        float percent;
        if (parameters.Length == 0)
        {
            // No percent: empty a full gauge, otherwise max it
            percent = current >= 100f ? 0f : 100f;
        }
        else if (parameters.Length == 1 && float.TryParse(parameters[0], out var parsed) && parsed >= 0f && parsed <= 100f)
        {
            percent = parsed;
        }
        else
        {
            SourceFeedback("Usage: supercharge [percent 0-100]", context);
            return;
        }

        context.Shard.SuperCharge.SetCharge(character, percent);

        SourceFeedback($"HKM gauge set to {percent:0.##}%, was {current:0.##}%", context);
    }
}
