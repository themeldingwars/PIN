namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemModule
{
    public uint ActionAbilityId { get; set; }
    public uint LocalizedNameModifierId { get; set; }
    public uint OrnamentsMapGroupId { get; set; }
    public uint ItemModuleType { get; set; }
    public uint ModuleAbilityId { get; set; }
    public uint OptionalGearModuleId { get; set; }
    public uint Id { get; set; }
    public byte ModuleLocation { get; set; }
    public byte IsPlayerUsable { get; set; }
}