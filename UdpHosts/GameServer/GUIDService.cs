namespace GameServer.Data;

public static class GuidService
{
    private const byte MainServerId = 31;
    private static uint MainCounter = 0;

    // https://github.com/themeldingwars/Documentation/wiki/Firefall-Guid-System#type-typecode
    public enum AdditionalTypes : byte
    {
        Instance = 0xFB,
        Army = 0xFC,
        Item = 0xFD,
        Character = 0xFE,
    }

    public static ulong GetNext(uint time, Enums.GSS.Controllers type = Enums.GSS.Controllers.Generic)
    {
        return GetNext(time, (byte)type);
    }

    public static ulong GetNext(uint time, byte type = (byte)Enums.GSS.Controllers.Generic)
    {
        return new Core.Data.EntityGuid(MainServerId, time, MainCounter++, type).Full;
    }

    public static ulong GetNext(IShard shard, Enums.GSS.Controllers type = Enums.GSS.Controllers.Generic)
    {
        return GetNext(shard, (byte)type);
    }

    public static ulong GetNext(IShard shard, byte type = (byte)Enums.GSS.Controllers.Generic)
    {
        return new Core.Data.EntityGuid(MainServerId, shard.CurrentTime, MainCounter++, type).Full;
    }
}