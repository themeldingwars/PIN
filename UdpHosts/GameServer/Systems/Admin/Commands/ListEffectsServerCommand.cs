using System.Text;
using GameServer.Aptitude;

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

        IAptitudeTarget target = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is IAptitudeTarget commandTarget)
        {
            target = commandTarget;
        }

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{target} Active Effects:");
        foreach (var activeEffect in target.GetActiveEffects())
        {
            if (activeEffect != null)
            {
                stringBuilder.AppendLine($"Idx {activeEffect.Index} : Effect {activeEffect.Effect?.Id}");
            }
        }

        string message = stringBuilder.ToString();
        context.SourcePlayer.SendDebugLog(message);
        context.SourcePlayer.SendDebugChat("Status list printed to console");
    }
}
