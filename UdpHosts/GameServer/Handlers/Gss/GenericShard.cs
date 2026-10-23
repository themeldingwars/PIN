using System;
using System.Linq;
using System.Numerics;
using Aero.Protocol;
using AeroMessages.Control;
using AeroMessages.GSS.Generic;
using GameServer.Entities;
using GameServer.Extensions;
using GameServer.GRPC;
using GameServer.Protocol;
using GameServer.Systems.Aptitude;
using Serilog;

namespace GameServer.Handlers.Gss;

/// <summary>
///     Handlers for messages on the root namespace typecode: they belong to the session rather than to a
///     game entity, so they reach the shard directly.
/// </summary>
[Typecode(GssTables.Ns.Root)]
public static class GenericShard
{
    private static readonly ILogger _logger = Log.ForContext(typeof(GenericShard));

    [MessageID(GssMessage.ScheduleUpdateRequest)]
    public static void ScheduleUpdateRequest(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var updateRequest = packet.Unpack<ScheduleUpdateRequest>();

        player.LastRequestedUpdate = shard.CurrentTime;
        player.RequestedClientTime = Math.Max(updateRequest.Time, player.RequestedClientTime);

        if (!player.FirstUpdateRequested)
        {
            player.FirstUpdateRequested = true;
            player.Respawn();
        }
    }

    [MessageID(GssMessage.UIToEncounterMessage)]
    public static void UiToEncounter(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.ServerProfiler_RequestNames)]
    public static void ServerProfiler_RequestNames(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.LocalProximityAbilitySuccess)]
    public static void LocalProximityAbilitySuccess(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var abilities = shard.Abilities;

        var message = packet.Unpack<LocalProximityAbilitySuccess>();
        shard.Entities.TryGetValue(message.Source.Backing & 0xffffffffffffff00, out IEntity sourceEntity);
        var source = (IAptitudeTarget)sourceEntity;
        var targets = message.Targets
        .Where(entityId =>
        {
            try
            {
                return shard.Entities[entityId.Backing & 0xffffffffffffff00] != null;
            }
            catch
            {
                return false;
            }
        })
        .Select(entityId => (IAptitudeTarget)shard.Entities[entityId.Backing & 0xffffffffffffff00])
        .ToArray();

        abilities.HandleLocalProximityAbilitySuccess(shard, source, message.ClientProximityCommandId, message.Time, new AptitudeTargets(targets));
    }

    [MessageID(GssMessage.RemoteProximityAbilitySuccess)]
    public static void RemoteProximityAbilitySuccess(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.TrailRequest)]
    public static void TrailRequest(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.RequestLeaveZone)]
    public static void RequestLeaveZone(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        _logger.Information("RequestLeaveZone from {CharacterId:X} in zone {ZoneId}", player.CharacterId, player.CurrentZone.ID);
    }

    [MessageID(GssMessage.RequestLogout)]
    public static void RequestLogout(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var resp = new CloseConnection { ShutdownCode = 0 };
        client.NetChannels[ChannelType.Control].SendMessage(resp);

        var zone = player.CurrentZone;

        if (!zone.IsOpenWorld)
        {
            return;
        }

        var playerPosition = player.CharacterEntity.Position;

        var minDistance = Vector3.DistanceSquared(playerPosition, zone.POIs["spawn"]);
        var closestOutpostId = zone.DefaultOutpostId;

        if (shard.Outposts.TryGetValue(zone.ID, out var outposts))
        {
            foreach (var outpost in outposts)
            {
                var distance = Vector3.DistanceSquared(playerPosition, outpost.Value.Outpost_ObserverView.PositionProp);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestOutpostId = outpost.Key;
                }
            }
        }

        _ = GRPCService.SaveCharacterSessionDataAsync(
              player.CharacterId + 0xFE,
              zone.ID,
              closestOutpostId,
              (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - player.ConnectedAt);
    }

    [MessageID(GssMessage.RequestEncounterInfo)]
    public static void RequestEncounterInfo(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.RequestActiveEncounters)]
    public static void RequestActiveEncounters(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.VotekickRequest)]
    public static void VotekickRequest(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.VotekickResponse)]
    public static void VotekickResponse(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.GlobalCounterRequest)]
    public static void GlobalCounterRequest(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.CurrentLoadoutRequest)]
    public static void CurrentLoadoutRequest(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.VendorProductRequest)]
    public static void VendorProductRequest(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
    }
}
