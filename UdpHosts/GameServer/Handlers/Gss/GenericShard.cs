using System;
using System.Linq;
using Aero.Protocol;
using AeroMessages.Common;
using AeroMessages.Control;
using AeroMessages.GSS.Generic;
using GameServer.Entities;
using GameServer.Extensions;
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
        var message = packet.Unpack<LocalProximityAbilitySuccess>();
        HandleProximity(shard, message.Source, message.ClientProximityCommandId, message.Targets, message.Time, local: true);
    }

    [MessageID(GssMessage.RemoteProximityAbilitySuccess)]
    public static void RemoteProximityAbilitySuccess(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var message = packet.Unpack<RemoteProximityAbilitySuccess>();
        HandleProximity(shard, message.Source, message.ClientProximityCommandId, message.Targets, message.Time, local: false);
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

        player.SaveSession();
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

    private static void HandleProximity(IShard shard, EntityId source, uint commandId, EntityId[] targets, uint time, bool local)
    {
        // Targets the client made up itself, e.g. its local effects, don't exist here
        IAptitudeTarget Lookup(EntityId id) => shard.Entities.TryGetValue(id.Backing & 0xffffffffffffff00, out var entity) ? entity as IAptitudeTarget : null;

        var found = targets.Select(Lookup).Where(target => target != null).ToArray();
        shard.Abilities.HandleProximityAbilitySuccess(shard, Lookup(source), commandId, time, found, local);
    }
}
