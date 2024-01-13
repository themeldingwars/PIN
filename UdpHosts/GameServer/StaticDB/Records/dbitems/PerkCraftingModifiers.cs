namespace GameServer.Data.SDB.Records.dbitems;
public record class PerkCraftingModifiers
{
    public uint CraftingModifierType { get; set; }
    public uint CraftingModifierValue { get; set; }
    public uint PerkModuleId { get; set; }
}
