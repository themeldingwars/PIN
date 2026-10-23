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
///     Entry point for the GSS channels. Decodes the GSS header (typecode, entity, message id), resolves
///     the route it describes, and invokes the <see cref="MessageIDAttribute" /> handler of the static
///     handler class registered for that route.
///     Runs on the thread of the shard that owns the client, driven by <see cref="Channel.Process" />.
/// </summary>
public static class GssRouter
{
    private static readonly ILogger _logger = Log.ForContext(typeof(GssRouter));

    private static readonly ProtocolRoute _characterRoute = new(GssTables.Ns.Character, GssTables.Kind.Command, typeof(GssCharacterCommand));
    private static readonly ProtocolRoute _vehicleRoute = new(GssTables.Ns.Vehicle, GssTables.Kind.Command, typeof(GssVehicleCommand));
    private static readonly ProtocolRoute _turretRoute = new(GssTables.Ns.Turret, GssTables.Kind.Command, typeof(GssTurretCommand));
    private static readonly ProtocolRoute _rootRoute = new(GssTables.Ns.Root, GssTables.Kind.Message, typeof(GssMessage));

    private static readonly ConcurrentDictionary<(int Ns, int ViewOrdinal), Type?> _handlerTypes = new();
    private static readonly ConcurrentDictionary<(int Ns, int ViewOrdinal, GssVersion Version), Dictionary<byte, MethodInfo>> _tables = new();

    /// <summary>
    ///     Handler methods are invoked with the receiving client, the shard handling it, the player the
    ///     client belongs to, the id of the entity the message targets and the packet positioned on the
    ///     message body.
    /// </summary>
    private static readonly Type[] _handlerParameters = [typeof(NetworkClient), typeof(IShard), typeof(IPlayer), typeof(ulong), typeof(GamePacket)];

    public static void Handle(GamePacket packet, NetworkClient client, IShard shard)
    {
        if (!packet.TryReadGssHeader(out var typecode, out var entityId, out var messageId))
        {
            _logger.Warning("---> Truncated GSS packet on {Channel}: {PacketBytes} bytes", packet.Header.Channel, packet.TotalBytes);
            return;
        }

        var version = shard.Settings.GssProtocolVersion;
        WireIds.ResolveGssRoute(version, typecode, out var ns, out var viewOrdinal);

        var handlerType = GetHandlerType(ns, viewOrdinal);

        if (handlerType == null)
        {
            _logger.Verbose("---> No handler for GSS typecode; Typecode = {Typecode} (ns-{Ns} view-{ViewOrdinal}) Entity = 0x{EntityId:X16} MsgID = {MessageId}!", typecode, ns, viewOrdinal, entityId, messageId);
            _logger.Warning(">  {PacketData}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
            return;
        }

        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("--> {Channel}: Typecode = {Typecode} ({TypecodeName}) Entity = 0x{EntityId:X16} MsgID = {MessageId}", packet.Header.Channel, typecode, handlerType.Name, entityId, messageId);
        }

        var route = GetProtocolRoute(ns);
        var table = GetDispatchTable(handlerType, ns, viewOrdinal, route, version);

        if (!table.TryGetValue(messageId, out var method))
        {
            _logger.Warning("Unhandled message {TypecodeName}::{MessageName} (tc-{Typecode} mid-{MessageId}) from Entity 0x{EntityId:X16}", handlerType.Name, GetUnhandledMessageLookup(route, version, messageId), GetTypecode(version, ns, viewOrdinal), messageId, entityId);
            _logger.Warning(">  {PacketData}", BitConverter.ToString(packet.Peek(packet.BytesRemaining).ToArray()).Replace("-", " "));
            return;
        }

        try
        {
            _ = method.Invoke(null, [client, shard, client.Player, entityId, packet]);
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
    ///     The static handler class registered for a GSS route, found by its <see cref="TypecodeAttribute" />.
    ///     Routes without a handler are cached too, so the search runs once per route.
    /// </summary>
    private static Type? GetHandlerType(int ns, int viewOrdinal)
    {
        if (_handlerTypes.TryGetValue((ns, viewOrdinal), out var cached))
        {
            return cached;
        }

        var type = ReflectionUtils.FindTypesByAttribute<TypecodeAttribute>().FirstOrDefault(t =>
        {
            var attr = t.GetAttribute<TypecodeAttribute>();

            return attr != null && attr.Namespace == ns && attr.ViewOrdinal == viewOrdinal;
        });

        _handlerTypes[(ns, viewOrdinal)] = type;

        return type;
    }

    /// <summary>
    ///     Wire id to handler method for a handler class in a protocol version. The handler classes are
    ///     static and the protocol version is fixed for the process, so one table serves every client.
    /// </summary>
    private static Dictionary<byte, MethodInfo> GetDispatchTable(Type handlerType, int ns, int viewOrdinal, ProtocolRoute? route, GssVersion version)
    {
        var key = (ns, viewOrdinal, version);
        if (_tables.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var table = new Dictionary<byte, MethodInfo>();

        if (route != null)
        {
            foreach (var method in ReflectionUtils.FindMethodsByAttribute<MessageIDAttribute>(handlerType))
            {
                var protocolId = method.GetAttribute<MessageIDAttribute>().ProtocolId;

                if (protocolId.GetType() != route.ProtocolEnum)
                {
                    continue;
                }

                if (!method.IsStatic || !method.GetParameters().Select(p => p.ParameterType).SequenceEqual(_handlerParameters))
                {
                    _logger.Error("Handler {HandlerType}.{MethodName} is not a static message handler and will never run", handlerType.Name, method.Name);
                    continue;
                }

                var wireId = GssTables.GetMessageId(version, route.Namespace, route.Kind, Convert.ToInt32(protocolId));

                if (wireId != 0)
                {
                    table[wireId] = method;
                }
            }
        }

        _tables[key] = table;
        return table;
    }

    private static ProtocolRoute? GetProtocolRoute(int ns)
    {
        switch (ns)
        {
            case GssTables.Ns.Character:
                return _characterRoute;
            case GssTables.Ns.Vehicle:
                return _vehicleRoute;
            case GssTables.Ns.Turret:
                return _turretRoute;
            case GssTables.Ns.Root:
                return _rootRoute;
            default:
                return null;
        }
    }

    private static byte GetTypecode(GssVersion version, int ns, int viewOrdinal)
    {
        return viewOrdinal >= 0
            ? GssTables.GetMessageId(version, ns, GssTables.Kind.View, viewOrdinal)
            : GssTables.GetNamespaceTypecode(version, ns);
    }

    private static string GetUnhandledMessageLookup(ProtocolRoute? route, GssVersion version, byte messageId)
    {
        if (route == null)
        {
            return "Unknown";
        }

        var lookupTypecode = GssTables.GetNamespaceTypecode(version, route.Namespace);
        var ordinal = GssTables.FindMessage(version, lookupTypecode, route.Kind, messageId);

        return ordinal >= 0 ? Enum.GetName(route.ProtocolEnum, ordinal) ?? "Unknown" : "Unknown";
    }

    private sealed record ProtocolRoute(int Namespace, int Kind, Type ProtocolEnum);
}
