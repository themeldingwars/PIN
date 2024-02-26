using System;
using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
[Flags]
public enum ItemFlags : uint
{
    IsTradable = 0x01,
    IsMailable = 0x02,
    Hidden = 0x04,
    Unk_0x08 = 0x08, // ?
    Resource = 0x10,
    Nopersist = 0x20,
    Cached = 0x40,
    IsPermanent = 0x80,
    Unk_0x100 = 0x100, // maybe "is_consumable" or "is_deletable"
    Unk_0x200 = 0x200, // Crystite (sdb_id 10) is the only item with this set
    Unk_0x400 = 0x400, // maybe developer only or something
    Unk_0x800 = 0x800, // maybe deprecated / unobtainable or broken
    Unk_0x1000 = 0x1000, // maybe "is_stackable"
    Unlimited = 0x2000,
    IsSalvageable = 0x4000,
    IsFancyNamed = 0x8000,
    IsPvp = 0x10000,
    Unk_0x20000 = 0x20000 // mostly pvp/regulation gear
}
