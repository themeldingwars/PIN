namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemSetAbilityEntries
{
    public uint AbilityChainId { get; set; }
    public uint NumOwned { get; set; }
    public uint ItemSetId { get; set; }
    public uint Id { get; set; }
}
