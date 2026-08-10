namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Remove all temporary equipment overrides", "tmpeq_clear", "tmpeq_clear", "tmpeqclear")]
public class TmpEqClearServerCommand : TmpEqServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var (success, character) = ResolveTargetCharacter(context);
        if (!success)
        {
            return;
        }

        var count = context.Service.ClearEquipmentOverrides(character.Player);
        if (count > 0)
        {
            character.ApplyLoadout(character.CurrentLoadout);
            SourceFeedback($"Cleared {count} equipment override(s) for {character}", context);
        }
        else
        {
            SourceFeedback($"No equipment overrides set for {character}", context);
        }
    }
}
