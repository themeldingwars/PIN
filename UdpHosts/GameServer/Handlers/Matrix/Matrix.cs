using Aero.Protocol;
using AeroMessages.Matrix;
using GameServer.Extensions;
using GameServer.Protocol;
using Serilog;
using Serilog.Events;

namespace GameServer.Handlers.Matrix;

/// <summary>
///     Handlers for the messages the client sends on the Matrix channel. Each one is marked with the
///     <see cref="MatrixMessage" /> it answers, the wire id of that message is resolved by <see cref="MatrixRouter" />.
/// </summary>
public static class Matrix
{
    private static readonly ILogger _logger = Log.ForContext(typeof(Matrix));

    [MessageID(MatrixMessage.Login)]
    public static void Login(NetworkClient client, IShard shard, IPlayer player, GamePacket packet)
    {
        var query = packet.Unpack<Login>();
        LoginService.BeginLogin(client, player, query.CharacterGuid, query.Ticket);
    }

    [MessageID(MatrixMessage.EnterZoneAck)]
    public static void EnterZoneAck(NetworkClient client, IShard shard, IPlayer player, GamePacket packet)
    {
        player.EnterZoneAck();
    }

    [MessageID(MatrixMessage.ExitZoneAck)]
    public static void ExitZoneAck(NetworkClient client, IShard shard, IPlayer player, GamePacket packet)
    {
        player.ExitZoneAck();
    }

    [MessageID(MatrixMessage.KeyframeRequest)]
    public static void KeyframeRequest(NetworkClient client, IShard shard, IPlayer player, GamePacket packet)
    {
        var query = packet.Unpack<KeyframeRequest>();

        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("KeyframeRequest with {EntityRequests} entity requests and {RefRequests} ref requests. Total scoped for player: {ScopedEntitiesForPlayer}",
            query.EntityRequests?.Length ?? 0,
            query.RefRequests?.Length ?? 0,
            shard.EntityMan.GetNumberOfScopedEntities(player));
        }

        if (query.EntityRequests == null)
        {
            return;
        }

        foreach (var request in query.EntityRequests)
        {
            byte typecode = (byte)(request.Entity & 0x00000000000000FFul);
            var entityId = request.Entity & 0xffffffffffffff00;
            shard.EntityMan.KeyframeRequest(client, player, entityId, typecode, request.Checksum);
        }
    }

    [MessageID(MatrixMessage.ClientStatus)]
    public static void ClientStatus(NetworkClient client, IShard shard, IPlayer player, GamePacket packet)
    {
        client.NetChannels[ChannelType.Matrix].SendMessage(new MatrixStatus
        {
            MatrixBytesPerSecond = 0,
            GameShapedBytes = 0,
            PacketUploss = 0,
            PacketDownloss = 0,
            MaxRecvSegmentBytes = 0,
            IsEverlastingGobsocket = 0,
            HaveUnk7 = 0,
            Unk8 = []
        });
    }

    [MessageID(MatrixMessage.LogInstrumentation)]
    public static void LogInstrumentation(NetworkClient client, IShard shard, IPlayer player, GamePacket packet)
    {
    }
}
