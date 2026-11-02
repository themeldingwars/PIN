using System.Threading.Tasks;
using GameServer.Data;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;
using GameServer.Systems.ZoneBorders;
using GameServer.Test;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class TeleportInstanceCommand : Command, ICommand
{
    private TeleportInstanceCommandDef Params;

    public TeleportInstanceCommand(TeleportInstanceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.ZoneId == 0)
        {
            result.SetPass();
            return;
        }

        if (!DataUtils.TryGetZone(Params.ZoneId, out var zone) || !zone.IsOpenWorld)
        {
            Logger.Debug("{Command} {CommandId} would move its targets into zone {ZoneId}, only open world zones are supported for now", nameof(TeleportInstanceCommand), Params.Id, Params.ZoneId);
            result.SetPass();
            return;
        }

        // The border effects run it in their remove chain without picking targets, it then moves the effect's owner
        var targets = context.Targets.Count > 0 ? [.. context.Targets] : new[] { context.Self };
        foreach (var target in targets)
        {
            if (target is CharacterEntity { Player: NetworkPlayer networkPlayer } character && zone.ID != networkPlayer.AssignedShard.ZoneId)
            {
                MoveToOpenWorld(networkPlayer, character, zone);
            }
        }

        result.SetPass();
        return;
    }

    private static void MoveToOpenWorld(NetworkPlayer player, CharacterEntity character, Zone zone)
    {
        var source = player.AssignedShard;
        var mapsPath = source.Settings.MapsPath;
        var arrival = ZoneGeometry.Get(source.ZoneId, mapsPath).ToNeighbor(character.Position, ZoneGeometry.Get(zone.ID, mapsPath))
                      ?? zone.POIs["spawn"];

        // The zone's shard may still have to load its collision, which takes a while, so not on this shard's thread
        _ = Task.Run(() =>
        {
            var shard = InstanceTransfer.Shards.GetOrCreateShard(zone.ID);
            source.Post(() => InstanceTransfer.TryStart(player, shard, arrival));
        });
    }
}
