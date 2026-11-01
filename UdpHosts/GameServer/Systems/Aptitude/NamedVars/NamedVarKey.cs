using FauFau.Util;
using GameServer.StaticDB;

namespace GameServer.Systems.Aptitude.NamedVars;

public readonly struct NamedVarKey
{
    private readonly uint _raw;

    private NamedVarKey(uint raw)
    {
        _raw = raw;
    }

    public ushort TypeId => (ushort)(_raw & 0xffff);
    public ushort NameHash => (ushort)(_raw >> 16);

    public static NamedVarKey FromDef(ushort nameId, string memberName)
    {
        var hash = string.IsNullOrEmpty(memberName) ? 0u : Checksum.FFnv32(memberName);
        return new NamedVarKey((((hash >> 16) ^ (hash & 0xffff)) << 16) | nameId);
    }

    public static NamedVarKey FromTypeId(ushort typeId)
    {
        return new NamedVarKey(typeId);
    }

    public static NamedVarKey FromStoreKey(uint raw)
    {
        return new NamedVarKey(raw);
    }

    public static string Describe(NamedVarKey key, string memberName)
    {
        return string.IsNullOrEmpty(memberName) ? key.ToString() : $"{memberName}/{key}";
    }

    public uint ToStoreKey()
    {
        return _raw;
    }

    public override string ToString()
    {
        var declared = SDBInterface.GetNamedVarTypes(TypeId);
        return declared is null ? $"unnamed({TypeId})" : $"{declared.Name}({TypeId})";
    }
}
