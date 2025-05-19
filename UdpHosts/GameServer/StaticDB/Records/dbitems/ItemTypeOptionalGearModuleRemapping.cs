namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemTypeOptionalGearModuleRemapping
{
    public uint SrcModuleId { get; set; }
    public uint UsedModuleId { get; set; }
    public uint CraftingTypeId { get; set; }
}