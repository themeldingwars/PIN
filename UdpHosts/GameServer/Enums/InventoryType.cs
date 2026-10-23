namespace GameServer.Enums;

public enum InventoryType : byte
{
    Unknown = 0,
    Bag = 1,
    Cache = 2, // Items that don't take up inventory space
    Crafting = 3,
    Gear = 4,
    Mail = 5,
    TempResources = 6
}