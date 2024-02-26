using System;
using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
[Flags]
public enum ItemDynamicFlags : uint
{
    IsBound = 0x01,
    Unk_0x02 = 0x02, // is_new?
    IsEquipped = 0x04
}
