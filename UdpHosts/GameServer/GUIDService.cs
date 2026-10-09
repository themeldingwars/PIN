using System.Threading;

namespace GameServer;

public static class GuidService
{
    private const byte _mainServerId = 31;
    private static uint _mainCounter;

    // https://github.com/themeldingwars/Documentation/wiki/Firefall-Guid-System#type-typecode
    public enum AdditionalTypes : byte
    {
        Instance = 0xFB,
        Army = 0xFC,
        Item = 0xFD,
        Character = 0xFE,
    }

    public static ulong GetNext(uint time, byte type = 0)
    {
        return new FauFau.Util.EntityGuid(_mainServerId, time, NextCounter(), type).Full;
    }

    public static ulong GetNext(IShard shard, byte type = 0)
    {
        return new FauFau.Util.EntityGuid(_mainServerId, shard.CurrentTime, NextCounter(), type).Full;
    }

    private static uint NextCounter()
    {
        return Interlocked.Increment(ref _mainCounter) - 1;
    }
}
