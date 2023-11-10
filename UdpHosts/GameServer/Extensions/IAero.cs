﻿using System;
using Aero.Gen;

namespace GameServer.Extensions;

// ReSharper disable once InconsistentNaming
internal static class IAeroExtensions
{
    /// <summary>
    ///     Serialize the instance to a region in memory
    /// </summary>
    /// <param name="aero"></param>
    /// <param name="serializedData"></param>
    internal static void SerializeToMemory(this IAero aero, out Memory<byte> serializedData)
    {
        serializedData = new Memory<byte>(new byte[aero.GetPackedSize()]);
        aero.Pack(serializedData.Span);
    }

    /// <summary>
    ///     Serialize the changes to a region in memory
    /// </summary>
    /// <param name="aero"></param>
    /// <param name="serializedData"></param>
    internal static void SerializeChangesToMemory(this IAeroViewInterface aero, out Memory<byte> serializedData)
    {
        serializedData = new Memory<byte>(new byte[aero.GetPackedChangesSize()]);
        aero.PackChanges(serializedData.Span);
    }
}