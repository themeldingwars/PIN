using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Heal your character", "healme [amount]", "healme")]
public class HealMeServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot heal without a valid player character", context);
            return;
        }

        if (parameters.Length > 1)
        {
            SourceFeedback("Usage: healme [amount]", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;

        if (!character.IsAlive)
        {
            SourceFeedback("You are dead", context);
            return;
        }

        int amount = parameters.Length == 1 ? (int)ParseUIntParameter(parameters[0]) : character.MaxHealth.Value - character.CurrentHealth;
        if (amount <= 0)
        {
            SourceFeedback(parameters.Length == 1 ? "Heal amount must be positive" : $"Already at full health: {character.CurrentHealth}/{character.MaxHealth.Value}", context);
            return;
        }

        context.Shard.Damage.ApplyHeal(character, amount);

        SourceFeedback($"Healed for {amount}. Health: {character.CurrentHealth}/{character.MaxHealth.Value}", context);
    }
}
