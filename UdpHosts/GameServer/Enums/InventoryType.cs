using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
public enum InventoryType : byte
{
    Unknown = 0,
    Bag = 1,
    Cache = 2,
    Crafting = 3,
    Gear = 4,
    Mail = 5,
    TempResources = 6
}
