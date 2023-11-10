namespace Core.Data;

/// <summary>
///     A Guid within the game (as described by SilentCLD).
///     <remarks>See https://gist.github.com/SilentCLD/881839a9f45578f1618db012fc789a71.</remarks>
/// </summary>
public readonly struct EntityGuid
{
    public readonly ulong Full;

    public EntityGuid(byte serverId, uint timestamp, uint counter, byte type)
    {
        ServerId = serverId;
        Timestamp = timestamp;
        Counter = counter;
        Type = type;
        Full = ((ulong)ServerId << 56) +
               ((ulong)((Timestamp >> 8) & 0x00FFFFFF) << 32) +
               (Counter << 8) +
               Type;
    }

    public byte ServerId { get; private init; }
    public uint Timestamp { get; private init; }
    public uint Counter { get; private init; }
    public byte Type { get; private init; }

    public static EntityGuid Parse(ulong guid)
    {
        EntityGuid entityGuid = new() { ServerId = (byte)(guid >> 56), Timestamp = (uint)(((guid >> 32) & 0x00FFFFFF) << 8), Counter = (uint)((guid & 0xFFFFFF00) >> 8), Type = (byte)(guid & 0xFF) };

        return entityGuid;
    }
}