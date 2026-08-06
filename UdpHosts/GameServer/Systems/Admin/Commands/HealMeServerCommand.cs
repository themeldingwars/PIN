using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Heal your character", "healme <amount>", "healme")]
public class HealMeServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot heal without a valid player character", context);
            return;
        }

        if (parameters.Length != 1)
        {
            SourceFeedback("Usage: healme <amount>", context);
            return;
        }

        int amount = (int)ParseUIntParameter(parameters[0]);
        if (amount <= 0)
        {
            SourceFeedback("Heal amount must be positive", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;

        if (!character.IsAlive)
        {
            SourceFeedback("You are dead", context);
            return;
        }

        context.Shard.Damage.ApplyHeal(character, amount);

        SourceFeedback($"Healed for {amount}. Health: {character.CurrentHealth}/{character.MaxHealth.Value}", context);
    }
}
