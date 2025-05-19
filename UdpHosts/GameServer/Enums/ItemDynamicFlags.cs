using System;

namespace GameServer.Enums;

[Flags]
public enum ItemDynamicFlags : byte
{
    IsBound = 0x01,
    Unk_0x02 = 0x02, // is_new?
    IsEquipped = 0x04
}
