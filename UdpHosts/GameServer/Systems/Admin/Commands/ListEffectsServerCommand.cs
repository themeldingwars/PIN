using System.Text;

namespace GameServer.Admin;

[ServerCommand("Log current status effects", "listeffects", "listeffects", "list_effects", "apt_status", "apt_list")]
public class ListEffectsServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot list effects without a valid player character", context);
            return;
        }

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("Active Effects:");
        foreach (var activeEffect in context.SourcePlayer.CharacterEntity.GetActiveEffects())
        {
            if (activeEffect != null)
            {
                stringBuilder.AppendLine($"{activeEffect.Index} : {activeEffect.Effect?.Id}");
            }
        }

        string message = stringBuilder.ToString();
        context.SourcePlayer.SendDebugLog(message);
        context.SourcePlayer.SendDebugChat("Status list printed to console");
    }
}
