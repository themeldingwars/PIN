using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Admin;

[ServerCommand("Set a target for server commands", "target [entityId/name]", "target")]
public class TargetServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null)
        {
            SourceFeedback("Cannot set target without a player", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        IEntity target = null;
        if (parameters.Length > 0)
        {
            // Attempt to set by params by looking for entity id or name
            var query = string.Concat(parameters);
            ulong maybeEntityId = ParseULongParameter(query);
            if (maybeEntityId != 0)
            {
                target = context.Shard.Entities.TryGetValue(maybeEntityId, out var value) ? value : null;
            }
            else
            {
                foreach (var pair in context.Shard.Entities)
                {
                    if (pair.Value is CharacterEntity chara)
                    {
                        if (chara.ToString() == query)
                        {
                            target = chara;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            // Attempt to set by ray cast
            var direction = character.AimDirection;
            var origin = character.GetProjectileOrigin(direction);
            (bool hit, Vector3 loc, ulong entId) = context.Shard.Physics.TargetRayCast(origin, direction, character);
            if (hit)
            {
                target = context.Shard.Entities.TryGetValue(entId, out var value) ? value : null;
            }
        }

        if (target == null)
        {
            SourceFeedback("Failed to find target", context);
        }
        else
        {
            context.Service.SetTarget(context.SourcePlayer, target);
            SourceFeedback($"Setting target to {target.AeroEntityId}", context);
        }
    }
}
