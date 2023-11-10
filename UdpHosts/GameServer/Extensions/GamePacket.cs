using Aero.Gen;
using GameServer.Packets;

namespace GameServer.Extensions;

internal static class GamePacketExtensions
{
    /// <summary>
    ///     Unpack the <see cref="GamePacket" /> into the provided class
    /// </summary>
    /// <typeparam name="T">The class to unpack into</typeparam>
    /// <param name="packet"></param>
    /// <returns></returns>
    /// <remarks>Do not call after other reading operations during endpoint execution! Relies heavily on <see cref="GamePacket.CurrentPosition" /> to work correctly</remarks>
    internal static T Unpack<T>(this GamePacket packet)
        where T : class, IAero, new()
    {
        var ret = new T();
        ret.Unpack(packet.PacketData.Span[packet.CurrentPosition..]);
        return ret;
    }
}