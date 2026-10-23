#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using Aero.Gen.Attributes;
using Aero.Protocol;

namespace GameServer.Protocol;

/// <summary>
///     Resolves wire ids (typecode + message id) for Aero message types against a protocol version,
///     using the versioned routing tables from Aero.Protocol.
/// </summary>
public static class WireIds
{
    private static readonly ConcurrentDictionary<(Type Type, GssVersion Version), GssWireId> GssCache = new();
    private static readonly ConcurrentDictionary<(Type Type, GssVersion Version, string ViewEnum, int ViewOrdinal), GssWireId> GssRouteCache = new();
    private static readonly ConcurrentDictionary<(Type Type, MatrixVersion Version), byte> MatrixCache = new();
    private static readonly ConcurrentDictionary<Type, ControlWireId> ControlCache = new();

    /// <summary>
    ///     Wire ids of a GSS message/command/view type in the given protocol version.
    ///     For views the typecode is the view typecode and the message id is 0.
    /// </summary>
    /// <param name="messageType">The Aero message, command or view type to resolve</param>
    /// <param name="version">The GSS protocol version to resolve against</param>
    /// <param name="typecode">The wire typecode of the type, 0 when the type does not resolve</param>
    /// <param name="messageId">The wire message id of the type, 0 for views and when the type does not resolve</param>
    /// <returns>true if the type has wire ids in this protocol version, false otherwise</returns>
    public static bool TryGetGssWireIds(Type messageType, GssVersion version, out byte typecode, out byte messageId)
    {
        var key = (messageType, version);
        if (!GssCache.TryGetValue(key, out var cached))
        {
            cached = ResolveGss(messageType, version, GetAeroMessageId(messageType));
            GssCache[key] = cached;
        }

        typecode = cached.Typecode;
        messageId = cached.MessageId;
        return cached.Valid;
    }

    /// <summary>
    ///     Wire ids of a GSS message on the route of one particular view or controller.
    /// </summary>
    /// <param name="messageType">The Aero message type to resolve</param>
    /// <param name="version">The GSS protocol version to resolve against</param>
    /// <param name="view">The view or controller the message is to be sent on</param>
    /// <param name="typecode">The route's wire typecode, 0 when the message is not sent on it</param>
    /// <param name="messageId">The wire message id of the type, 0 when the message is not sent on the route</param>
    /// <returns>true if the message has wire ids on that route in this protocol version, false otherwise</returns>
    public static bool TryGetGssWireIdsOnView(Type messageType, GssVersion version, Enum view, out byte typecode, out byte messageId)
    {
        var viewEnum = view.GetType().Name;
        var viewOrdinal = Convert.ToInt32(view);
        var key = (messageType, version, viewEnum, viewOrdinal);

        if (!GssRouteCache.TryGetValue(key, out var cached))
        {
            var attribute = GetAeroMessageIds(messageType)
                .FirstOrDefault(entry => entry.ViewEnum == viewEnum && entry.ViewOrdinal == viewOrdinal);

            cached = ResolveGss(messageType, version, attribute);
            GssRouteCache[key] = cached;
        }

        typecode = cached.Typecode;
        messageId = cached.MessageId;
        return cached.Valid;
    }

    /// <summary>
    ///     Wire id of a matrix message type in the given protocol version.
    /// </summary>
    /// <param name="messageType">The Aero matrix message type to resolve</param>
    /// <param name="version">The matrix protocol version to resolve against</param>
    /// <param name="messageId">The matrix wire id of the message type, 0 when the type does not resolve</param>
    /// <returns>true if the type is a matrix message in this protocol version, false otherwise</returns>
    public static bool TryGetMatrixWireId(Type messageType, MatrixVersion version, out byte messageId)
    {
        var key = (messageType, version);
        if (MatrixCache.TryGetValue(key, out var cached))
        {
            messageId = cached;
            return cached != 0;
        }

        var result = ResolveMatrix(messageType, version);
        MatrixCache[key] = result;

        messageId = result;
        return result != 0;
    }

    /// <summary>
    ///     Wire id of a control message type. Control messages are not version routed: the message id
    ///     from the AeroMessageId attribute is the wire id.
    /// </summary>
    /// <param name="messageType">The Aero message type to resolve</param>
    /// <param name="messageId">The control wire id of the message type, 0 when the type does not resolve</param>
    /// <returns>true if the type is a control message, false otherwise</returns>
    public static bool TryGetControlWireId(Type messageType, out byte messageId)
    {
        if (ControlCache.TryGetValue(messageType, out var cached))
        {
            messageId = cached.MessageId;
            return cached.Valid;
        }

        var attr = GetAeroMessageId(messageType);
        var result = attr is { Typ: AeroMessageIdAttribute.MsgType.Control }
                         ? new ControlWireId((byte)attr.MessageId, true)
                         : default;
        ControlCache[messageType] = result;

        messageId = result.MessageId;
        return result.Valid;
    }

    /// <summary>
    ///     Universal (namespace, view ordinal) route carried by a wire typecode in the given protocol version.
    ///     View routes resolve to their view ordinal, namespace routes to a view ordinal of -1.
    ///     Typecodes that match no known route fall back to the root namespace: the client sends
    ///     root-namespace messages on typecode 0 as well as on the unrouted shard typecode (251).
    /// </summary>
    /// <param name="version">The GSS protocol version the typecode comes from</param>
    /// <param name="typecode">The wire typecode to route</param>
    /// <param name="nsIndex">The universal namespace the typecode belongs to</param>
    /// <param name="viewOrdinal">The view ordinal the typecode carries, -1 for a namespace route or an unknown typecode</param>
    public static void ResolveGssRoute(GssVersion version, byte typecode, out int nsIndex, out int viewOrdinal)
    {
        if (GssTables.TryFindView(version, typecode, out nsIndex, out viewOrdinal))
        {
            return;
        }

        var ns = GssTables.FindNamespace(version, typecode, GssTables.Kind.Message);
        if (ns != GssTables.Ns.Unknown)
        {
            nsIndex = ns;
            viewOrdinal = -1;
            return;
        }

        nsIndex = GssTables.Ns.Root;
        viewOrdinal = -1;
    }

    /// <summary>
    ///     Resolves a GSS type's wire ids from its AeroMessageId attribute and the versioned routing tables.
    /// </summary>
    /// <param name="messageType">The Aero message, command or view type to resolve</param>
    /// <param name="version">The GSS protocol version to resolve against</param>
    /// <param name="attr">The route declaration to resolve, null when the type has none for what was asked</param>
    /// <returns>The wire ids, or a default result when the route is absent from this protocol version</returns>
    private static GssWireId ResolveGss(Type messageType, GssVersion version, AeroMessageIdAttribute? attr)
    {
        if (attr is not { Typ: AeroMessageIdAttribute.MsgType.GSS, MessageEnum: not null })
        {
            return default;
        }

        var versionTo = attr.VersionTo < 0 ? GssTables.VersionCount - 1 : attr.VersionTo;
        if ((int)version < attr.VersionFrom || (int)version > versionTo)
        {
            return default;
        }

        if (!GssTables.TryGetProtocolEnumInfo(attr.MessageEnum, out var nsIndex, out var kind))
        {
            throw new InvalidOperationException($"Unknown protocol enum {attr.MessageEnum} for type {messageType.FullName}");
        }

        if (kind == GssTables.Kind.View)
        {
            var viewTypecode = GssTables.GetMessageId(version, nsIndex, kind, attr.MessageOrdinal);
            return viewTypecode != 0 ? new GssWireId(viewTypecode, 0, true) : default;
        }

        var messageId = GssTables.GetMessageId(version, nsIndex, kind, attr.MessageOrdinal);
        if (messageId == 0)
        {
            return default;
        }

        byte typecode;
        if (attr.ViewEnum is { Length: > 0 } viewEnum)
        {
            // Messages ride on the route typecode of the view/controller that carries them.
            if (!GssTables.TryGetProtocolEnumInfo(viewEnum, out var viewNs, out var viewKind)
                || viewKind != GssTables.Kind.View)
            {
                throw new InvalidOperationException($"Invalid view route {viewEnum} for type {messageType.FullName}");
            }

            typecode = GssTables.GetMessageId(version, viewNs, GssTables.Kind.View, attr.ViewOrdinal);
            if (typecode == 0)
            {
                return default;
            }
        }
        else
        {
            typecode = GssTables.GetNamespaceTypecode(version, nsIndex);
            if (typecode == 255)
            {
                return default;
            }
        }

        return new GssWireId(typecode, messageId, true);
    }

    /// <summary>
    ///     Resolves a matrix message type's wire id from its AeroMessageId attribute and the versioned matrix table.
    /// </summary>
    /// <param name="messageType">The Aero matrix message type to resolve</param>
    /// <param name="version">The matrix protocol version to resolve against</param>
    /// <returns>The wire id, 0 when the type is absent from this protocol version</returns>
    private static byte ResolveMatrix(Type messageType, MatrixVersion version)
    {
        if (GetAeroMessageId(messageType) is not { Typ: AeroMessageIdAttribute.MsgType.Matrix, MessageEnum: not null } attr)
        {
            return 0;
        }

        var versionTo = attr.VersionTo < 0 ? MatrixTables.VersionCount - 1 : attr.VersionTo;
        if ((int)version < attr.VersionFrom || (int)version > versionTo)
        {
            return 0;
        }

        if (attr.MessageEnum != nameof(MatrixMessage))
        {
            return 0;
        }

        return MatrixTables.GetMessageId(version, (MatrixMessage)attr.MessageOrdinal);
    }

    /// <summary>
    ///     Reads the first AeroMessageId attribute a message type carries, which is its default route.
    /// </summary>
    /// <param name="messageType">The type to read the attribute from</param>
    /// <returns>The attribute, null when the type carries none</returns>
    private static AeroMessageIdAttribute? GetAeroMessageId(Type messageType)
    {
        return GetAeroMessageIds(messageType).FirstOrDefault();
    }

    /// <summary>
    ///     Reads every AeroMessageId attribute a message type carries. A type sent to more than one kind of
    ///     recipient carries one per route it rides on, in declaration order.
    /// </summary>
    /// <param name="messageType">The type to read the attributes from</param>
    /// <returns>The attributes, empty when the type carries none</returns>
    private static AeroMessageIdAttribute[] GetAeroMessageIds(Type messageType)
    {
        return messageType.GetCustomAttributes(typeof(AeroMessageIdAttribute), false).Cast<AeroMessageIdAttribute>().ToArray();
    }

    private readonly record struct GssWireId(byte Typecode, byte MessageId, bool Valid);

    private readonly record struct ControlWireId(byte MessageId, bool Valid);
}
