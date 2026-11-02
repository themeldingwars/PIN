using System.Numerics;
using System.Threading.Tasks;
using GameServer.Test;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Move into a new instance of a zone, /leavezone goes back to the open world", "instance <zone id>", "instance")]
public class InstanceServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var player = context.SourcePlayer;
        if (parameters.Length != 1 || player?.CharacterEntity == null)
        {
            SourceFeedback("Usage: instance <zone id>", context);
            return;
        }

        var source = player.AssignedShard;
        var zoneId = ParseUIntParameter(parameters[0]);
        if (!DataUtils.TryGetZone(zoneId, out var target))
        {
            SourceFeedback($"Zone {zoneId} isn't known", context);
            return;
        }

        SourceFeedback($"Creating an instance of {target.Name}", context);

        // Loading the collision takes a while, not on the shard thread
        _ = Task.Run(() =>
        {
            var instance = InstanceTransfer.Shards.CreateInstance(zoneId);
            source.Post(() => InstanceTransfer.TryStart(player, instance, target.POIs.TryGetValue("spawn", out var spawn) ? spawn : Vector3.Zero));
        });
    }
}
