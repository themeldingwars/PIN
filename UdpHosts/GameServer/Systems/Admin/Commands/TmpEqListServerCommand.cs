namespace GameServer.Systems.Admin.Commands;

[ServerCommand("List temporary equipment overrides", "tmpeq", "tmpeq", "tmpeq_list", "tmpeqlist")]
public class TmpEqListServerCommand : TmpEqServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var (success, character) = ResolveTargetCharacter(context);
        if (!success)
        {
            return;
        }

        SourceFeedback($"Equipment overrides for {character}: {FormatOverrides(context, character.Player)}", context);
    }
}
