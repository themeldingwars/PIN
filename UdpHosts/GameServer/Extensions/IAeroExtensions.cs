using System;
using System.IO.Hashing;
using Aero.Gen;

namespace GameServer.Extensions;

// ReSharper disable once InconsistentNaming
internal static class IAeroExtensions
{
    /// <summary>
    ///     Serialize the instance to a region in memory
    /// </summary>
    /// <param name="aero">Any AeroMessage class implementing AeroMessageId</param>
    /// <param name="serializedData">Output</param>
    internal static void SerializeToMemory(this IAero aero, out Memory<byte> serializedData)
    {
        serializedData = new Memory<byte>(new byte[aero.GetPackedSize()]);
        aero.Pack(serializedData.Span);
    }

    /// <summary>
    ///     Serialize the changes to a region in memory
    /// </summary>
    /// <param name="aero">View or Controller implementing AeroMessageId</param>
    /// <param name="serializedData">Output</param>
    internal static void SerializeChangesToMemory(this IAeroViewInterface aero, out Memory<byte> serializedData)
    {
        serializedData = new Memory<byte>(new byte[aero.GetPackedChangesSize()]);
        aero.PackChanges(serializedData.Span);
    }

    /// <summary>
    ///     Get the checksum
    /// </summary>
    /// <param name="aero">View or Controller implementing AeroMessageId</param>
    /// <returns>The checksum</returns>
    internal static uint SerializeToChecksum(this IAeroViewInterface aero)
    {
        var serializedData = new Memory<byte>(new byte[aero.GetPackedSize()]);
        aero.Pack(serializedData.Span);
        return Crc32.HashToUInt32(serializedData.Span);
    }
}