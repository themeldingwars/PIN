namespace GameServer.Data.SDB.Records.dbitems;
public record class SalvageDisplayItems
{
    public uint SalvageRewardsId { get; set; }
    public uint ItemId { get; set; }
    public byte Guaranteed { get; set; }
}