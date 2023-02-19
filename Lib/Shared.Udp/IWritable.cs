using System;

namespace Shared.Udp;

public interface IWritable
{
    Memory<byte> Write();
    Memory<byte> WriteBE();
}