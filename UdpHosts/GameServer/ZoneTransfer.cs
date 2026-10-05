using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GameServer.Data;
using GameServer.GRPC;
using GameServer.Test;
using Serilog;

namespace GameServer;

/// <summary>
///     Moves a player to another zone. The zone is saved as the character's last zone and the connection closed;
///     the client then fetches a new ticket and logs in again, which puts the character in that zone's shard.
/// </summary>
public static class ZoneTransfer
{
    private static readonly ILogger _logger = Log.ForContext(typeof(ZoneTransfer));
    private static readonly ConcurrentDictionary<ulong, byte> _pending = new();

    public static bool TryStart(INetworkPlayer player, uint zoneId, out string refusal)
    {
        refusal = Validate(player, zoneId, out var zone);
        if (refusal != null)
        {
            return false;
        }

        if (!_pending.TryAdd(player.CharacterId, 0))
        {
            refusal = "A transfer is already in progress";
            return false;
        }

        _ = TransferAsync(player, zone);
        return true;
    }

    private static string Validate(INetworkPlayer player, uint zoneId, out Zone zone)
    {
        zone = null;
        var character = player.CharacterEntity;

        if (character == null || player.CurrentZone == null)
        {
            return "You aren't in a zone yet";
        }

        if (!DataUtils.TryGetZone(zoneId, out zone))
        {
            return $"Zone {zoneId} isn't known";
        }

        if (!zone.IsOpenWorld)
        {
            return $"{zone.Name} is an instance and can't be travelled to";
        }

        if (zone.ID == player.CurrentZone.ID)
        {
            return $"You are already in {zone.Name}";
        }

        if (!character.Alive)
        {
            return "You can't travel while dead";
        }

        if (character.IsAttached)
        {
            return "You can't travel from a vehicle or turret";
        }

        return null;
    }

    private static async Task TransferAsync(INetworkPlayer player, Zone zone)
    {
        try
        {
            // RIN identifies characters by the full guid, which has the character type in its last byte
            var characterGuid = player.CharacterId + (byte)GuidService.AdditionalTypes.Character;
            var timePlayed = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - player.ConnectedAt;

            if (!await GRPCService.TransferCharacterAsync(characterGuid, zone.ID, 0, timePlayed))
            {
                _logger.Error("RIN didn't save the transfer of {CharacterId:X} to zone {ZoneId}", player.CharacterId, zone.ID);
                player.SendDebugChat($"Transfer to {zone.Name} failed");
                return;
            }

            _logger.Information("Transferring {CharacterId:X} to zone {ZoneId}", player.CharacterId, zone.ID);
            var close = new AeroMessages.Control.CloseConnection { ShutdownCode = 0 };
            player.NetChannels[ChannelType.Control].SendMessage(close);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Couldn't transfer {CharacterId:X} to zone {ZoneId}", player.CharacterId, zone.ID);
            player.SendDebugChat($"Transfer to {zone.Name} failed");
        }
        finally
        {
            _pending.TryRemove(player.CharacterId, out _);
        }
    }
}
