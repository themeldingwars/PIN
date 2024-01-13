namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemPrestige
{
    public uint Cost { get; set; }
    public float ResearchPointsMultiplier { get; set; }
    public uint LocalizedId { get; set; }
    public byte Level { get; set; }
}
