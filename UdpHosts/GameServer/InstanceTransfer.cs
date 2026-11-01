using System;
using System.Numerics;
using System.Threading.Tasks;
using AeroMessages.Matrix;
using GameServer.Protocol;
using GameServer.Systems.CharacterCreation;
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

    public sealed record Pending(IShard Target, Vector3 Position);
}
