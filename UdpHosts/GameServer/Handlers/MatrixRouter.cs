#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aero.Protocol;
using GameServer.Extensions;
using GameServer.Protocol;
using Serilog;
using Serilog.Events;

namespace GameServer.Handlers;

/// <summary>
///     Entry point for the Matrix channel. Resolves the wire id to a <see cref="MatrixMessage" /> and invokes
///     the <see cref="MessageIDAttribute" /> handler registered for it.
///     Runs on the thread of the shard that owns the client, driven by <see cref="Channel.Process" />.
/// </summary>
public static class MatrixRouter
{
    private static readonly ILogger _logger = Log.ForContext(typeof(MatrixRouter));

    private static readonly ConcurrentDictionary<MatrixVersion, Dictionary<byte, MethodInfo>> _tables = new();

    /// <summary>
    ///     Handler methods are invoked with the receiving client, the shard handling it, the player the
    ///     client belongs to and the packet positioned on the message body.
    /// </summary>
    private static readonly Type[] _handlerParameters = [typeof(NetworkClient), typeof(IShard), typeof(IPlayer), typeof(GamePacket)];

    public static void Handle(GamePacket packet, NetworkClient client, IShard shard)
    {
        if (packet.BytesRemaining < 1)
        {
            _logger.Warning("---> Truncated Matrix packet: {PacketBytes} bytes", packet.TotalBytes);
            return;
        }

        var version = shard.Settings.MatrixProtocolVersion;
        var wireId = packet.ReadByte();

        if (!GetDispatchTable(version).TryGetValue(wireId, out var method))
        {
            _logger.Warning("---> Unhandled Matrix Packet {MessageName} (mid-{MessageId}) on protocol {Version}!!!", GetMessageName(version, wireId), wireId, version);
            _logger.Warning(">  {PacketData}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
            return;
        }

        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("--> {Channel}: MsgID = {Message} ({MessageId})", ChannelType.Matrix, GetMessageName(version, wireId), wireId);
        }

        try
        {
            _ = method.Invoke(null, [client, shard, client.Player, packet]);
        }
        catch (TargetInvocationException e)
        {
            if (e.InnerException != null)
            {
                _logger.Error("HandlePacket Caught {ExceptionMessage}", e.InnerException.Message);
                _logger.Error("{StackTrace}", e.InnerException.StackTrace);
            }
        }
    }

    /// <summary>
    ///     Wire id to handler method in a protocol version, collected from every <see cref="MessageIDAttribute" />
    ///     naming a <see cref="MatrixMessage" />. Handler classes are static and the protocol version is fixed
    ///     for the process, so one table serves every client.
    /// </summary>
    private static Dictionary<byte, MethodInfo> GetDispatchTable(MatrixVersion version)
    {
        if (_tables.TryGetValue(version, out var cached))
        {
            return cached;
        }

        var table = new Dictionary<byte, MethodInfo>();

        foreach (var method in ReflectionUtils.FindMethodsByAttribute<MessageIDAttribute>())
        {
            if (method.GetAttribute<MessageIDAttribute>().ProtocolId is not MatrixMessage messageId)
            {
                continue;
            }

            if (!method.IsStatic || !method.GetParameters().Select(p => p.ParameterType).SequenceEqual(_handlerParameters))
            {
                _logger.Error("Handler {HandlerType}.{MethodName} is not a static matrix message handler and will never run", method.DeclaringType?.Name, method.Name);
                continue;
            }

            var handlerWireId = MatrixTables.GetMessageId(version, messageId);

            if (handlerWireId != 0)
            {
                table[handlerWireId] = method;
            }
        }

        _tables[version] = table;
        return table;
    }

    private static string GetMessageName(MatrixVersion version, byte wireId)
    {
        var ordinal = MatrixTables.FindMessage(version, wireId);

        return ordinal >= 0 ? Enum.GetName((MatrixMessage)ordinal) ?? "Unknown" : "Unknown";
    }
}
