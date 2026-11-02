using System;
using System.Numerics;
using System.Threading.Tasks;
using AeroMessages.Matrix;
using GameServer.Protocol;
using GameServer.Systems.CharacterCreation;
using GameServer.Test;
using Serilog;

namespace GameServer;

/// <summary>
///     Moves a player to another shard of this process on the same connection, the way the live servers moved players
///     between zones: ExitZone, then on its ack the character is built again on the target shard from RIN and EnterZone
///     puts it at the given position. Used for instances like a Melding pocket, where a relogin through RIN can't name
///     the shard.
/// </summary>
public static class InstanceTransfer
{
    private static readonly ILogger _logger = Log.ForContext(typeof(InstanceTransfer));

    public static IShardManager Shards { get; private set; }

    public static void Init(IShardManager shards)
    {
        Shards = shards;
    }

    /// <summary>Starts the move, on the thread of the player's shard. False when the player can't move now.</summary>
    public static bool TryStart(INetworkPlayer player, IShard target, Vector3 position)
    {
        if (player is not NetworkPlayer networkPlayer || networkPlayer.PendingTransfer != null || networkPlayer.CharacterEntity == null)
        {
            return false;
        }

        if (IsOpenWorld(player.AssignedShard.ZoneId) && !IsOpenWorld(target.ZoneId))
        {
            networkPlayer.ReturnPoint = new ReturnPoint(player.AssignedShard.ZoneId, networkPlayer.CharacterEntity.Position);
        }

        // Only saves in the open world, so the character's zone in RIN stays the one it came from
        networkPlayer.SaveSession();
        networkPlayer.PendingTransfer = new Pending(target, position);
        networkPlayer.NetChannels[ChannelType.Matrix].SendMessage(new ExitZone());
        _logger.Information("Moving {CharacterId:X} from zone {FromZone} to {ShardId:X16} of zone {ToZone}", player.CharacterId, player.AssignedShard.ZoneId, target.InstanceId, target.ZoneId);
        return true;
    }

    /// <summary>Called for ExitZoneAck. False when no move is pending, the ack then just leaves the zone.</summary>
    internal static bool TryComplete(NetworkPlayer player)
    {
        if (player.PendingTransfer is not { } pending)
        {
            return false;
        }

        player.PendingTransfer = null;
        player.AssignedShard.MigrateOut(player);
        player.CharacterEntity = null;
        pending.Target.TransferIn(player);

        var accountId = (long)player.PlayerId;
        var characterGuid = player.CharacterId + (byte)GuidService.AdditionalTypes.Character;
        _ = Task.Run(async () =>
        {
            try
            {
                var data = await LoginService.RetrieveCharacterAsync(characterGuid);
                pending.Target.EventBus.Enqueue(new CharacterDataFetchedEvent(player, data, accountId, pending.Position));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't move {CharacterId:X} to zone {ZoneId}", player.CharacterId, pending.Target.ZoneId);
                player.Close("instance transfer failed");
            }
        });

        return true;
    }

    /// <summary>
    ///     Takes a player in an instance back to the open world spot they left it from, or to the spawn point of the
    ///     server's zone when they didn't come from one, e.g. after logging in there. False in the open world, where the
    ///     client's "Leave Zone" is disabled as well, and while a move is still going on.
    /// </summary>
    public static bool TryReturnToOpenWorld(INetworkPlayer player)
    {
        if (player is not NetworkPlayer networkPlayer || networkPlayer.CharacterEntity == null || networkPlayer.PendingTransfer != null
            || IsOpenWorld(networkPlayer.AssignedShard.ZoneId))
        {
            return false;
        }

        var back = networkPlayer.ReturnPoint ?? new ReturnPoint(Shards.DefaultShard.ZoneId, DataUtils.GetZone(Shards.DefaultShard.ZoneId).POIs["spawn"]);
        networkPlayer.ReturnPoint = null;

        // The zone's shard may still have to load its collision, which takes a while, so not on this shard's thread
        var source = networkPlayer.AssignedShard;
        _ = Task.Run(() =>
        {
            var shard = Shards.GetOrCreateShard(back.ZoneId);
            source.Post(() => TryStart(networkPlayer, shard, back.Position));
        });

        return true;
    }

    private static bool IsOpenWorld(uint zoneId) => DataUtils.TryGetZone(zoneId, out var zone) && zone.IsOpenWorld;

    public sealed record Pending(IShard Target, Vector3 Position);

    /// <summary>The open world spot a player left for an instance, where leaving the instance takes them back.</summary>
    public sealed record ReturnPoint(uint ZoneId, Vector3 Position);
}
