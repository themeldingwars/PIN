using GameServer.Systems.Progression;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Award experience to your current battleframe", "givexp <amount>", "givexp", "give_xp", "addxp")]
public class GiveXpServerCommand : ProgressionServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var (success, player) = ResolveSourcePlayer(context);
        if (!success)
        {
            return;
        }

        if (parameters.Length == 0)
        {
            SourceFeedback("Usage: givexp <amount>", context);
            return;
        }

        uint amount = ParseUIntParameter(parameters[0]);
        if (amount == 0)
        {
            SourceFeedback("The experience amount has to be greater than 0", context);
            return;
        }

        if (context.Shard.Progression.AwardXp(player, amount, uiType: XpRewardUiType.CmdAddXp) is not ProgressionUpdate update)
        {
            SourceFeedback("Could not award the experience", context);
            return;
        }

        SourceFeedback(FormatProgression($"Awarded {amount} xp", update), context);
    }
}
