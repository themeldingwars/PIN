using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GameServer.Test;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Move into a new instance of a zone, or back to the open world zone with \"instance leave\"", "instance <zone id> | instance leave", "instance")]
public class InstanceServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var player = context.SourcePlayer;
        if (parameters.Length != 1 || player?.CharacterEntity == null)
        {
            SourceFeedback("Usage: instance <zone id> | instance leave", context);
            return;
        }

        var source = player.AssignedShard;
        if (parameters[0] == "leave")
        {
            var home = InstanceTransfer.Shards.Shards.FirstOrDefault(shard => shard != source && DataUtils.TryGetZone(shard.ZoneId, out var zone) && zone.IsOpenWorld);
            if (home == null || !InstanceTransfer.TryStart(player, home, DataUtils.GetZone(home.ZoneId).POIs["spawn"]))
            {
                SourceFeedback("There's no open world shard to go back to", context);
            }

            return;
        }

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
