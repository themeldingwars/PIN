using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Take damage on your character", "hurtme <amount>", "hurtme")]
public class HurtMeServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot take damage without a valid player character", context);
            return;
        }

        if (parameters.Length != 1)
        {
            SourceFeedback("Usage: hurtme <amount>", context);
            return;
        }

        int amount = (int)ParseUIntParameter(parameters[0]);
        if (amount <= 0)
        {
            SourceFeedback("Damage amount must be positive", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;

        if (!character.IsAlive)
        {
            SourceFeedback("You are dead", context);
            return;
        }

        context.Shard.Damage.ApplyDamage(character, amount);

        SourceFeedback($"Took {amount} damage. Health: {character.CurrentHealth}/{character.MaxHealth.Value}", context);
    }
}
