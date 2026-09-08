using System;
using System.Linq;
using System.Numerics;
using Aero.Protocol;
using AeroMessages.Control;
using AeroMessages.GSS.Generic;
using GameServer.Entities;
using GameServer.Extensions;
using GameServer.GRPC;
using GameServer.Packets;
using GameServer.Systems.Aptitude;
using Serilog;

namespace GameServer.Controllers;

[Typecode(GssTables.Ns.Root)]
public class GenericShard : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
    }

    [MessageID(GssMessage.ScheduleUpdateRequest)]
    public void ScheduleUpdateRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var updateRequest = packet.Unpack<ScheduleUpdateRequest>();

        player.LastRequestedUpdate = client.AssignedShard.CurrentTime;
        player.RequestedClientTime = Math.Max(updateRequest.Time, player.RequestedClientTime);

        if (!player.FirstUpdateRequested)
        {
            player.FirstUpdateRequested = true;
            player.Respawn();
        }
    }

    [MessageID(GssMessage.UIToEncounterMessage)]
    public void UiToEncounter(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.ServerProfiler_RequestNames)]
    public void ServerProfiler_RequestNames(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.LocalProximityAbilitySuccess)]
    public void LocalProximityAbilitySuccess(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var shard = client.AssignedShard;
        var abilities = client.AssignedShard.Abilities;

        var message = packet.Unpack<LocalProximityAbilitySuccess>();
        shard.Entities.TryGetValue(message.Source.Backing & 0xffffffffffffff00, out IEntity sourceEntity);
        var source = (IAptitudeTarget)sourceEntity;
        var targets = message.Targets
        .Select(entityId =>
        {
            shard.Entities.TryGetValue(entityId.Backing & 0xffffffffffffff00, out var target);
            return target as IAptitudeTarget;
        })
        .Where(target => target != null)
        .ToArray();

        abilities.HandleLocalProximityAbilitySuccess(shard, source, message.ClientProximityCommandId, message.Time, new AptitudeTargets(targets));
    }

    [MessageID(GssMessage.RemoteProximityAbilitySuccess)]
    public void RemoteProximityAbilitySuccess(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.TrailRequest)]
    public void TrailRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.RequestLeaveZone)]
    public void RequestLeaveZone(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.RequestLogout)]
    public void RequestLogout(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var resp = new CloseConnection { Unk = [0, 0, 0, 0] };
        client.NetChannels[ChannelType.Control].SendMessage(resp);

        var zone = player.CurrentZone;

        if (!zone.IsOpenWorld)
        {
            return;
        }

        var playerPosition = player.CharacterEntity.Position;

        var minDistance = Vector3.DistanceSquared(playerPosition, zone.POIs["spawn"]);
        var closestOutpostId = zone.DefaultOutpostId;

        if (client.AssignedShard.Outposts.TryGetValue(zone.ID, out var outposts))
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
    public void RequestEncounterInfo(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.RequestActiveEncounters)]
    public void RequestActiveEncounters(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.VotekickRequest)]
    public void VotekickRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.VotekickResponse)]
    public void VotekickResponse(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.GlobalCounterRequest)]
    public void GlobalCounterRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.CurrentLoadoutRequest)]
    public void CurrentLoadoutRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }

    [MessageID(GssMessage.VendorProductRequest)]
    public void VendorProductRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
    }
}