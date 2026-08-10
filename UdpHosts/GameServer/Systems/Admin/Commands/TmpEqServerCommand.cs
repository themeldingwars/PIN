using System.Linq;
using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

public abstract class TmpEqServerCommand : ServerCommand
{
    protected (bool Success, CharacterEntity Character) ResolveTargetCharacter(ServerCommandContext context)
    {
        var character = context.Target as CharacterEntity;
        if (character != null && character.Player == null)
        {
            SourceFeedback("Target is not a player-controlled character", context);
            return (false, null);
        }

        if (character == null)
        {
            character = context.SourcePlayer?.CharacterEntity;
            if (character == null)
            {
                SourceFeedback("No valid character to modify", context);
                return (false, null);
            }
        }

        return (true, character);
    }

    protected string FormatOverrides(ServerCommandContext context, INetworkPlayer player)
    {
        var overrides = context.Service.GetEquipmentOverrides(player);
        if (overrides == null)
        {
            return "no active overrides";
        }

        return string.Join(", ", overrides.Select(o => $"{o.Key}={o.Value}"));
    }
}
