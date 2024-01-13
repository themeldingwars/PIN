namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemArrayElement
{
    public float Multiplier { get; set; }
    public int Quantity { get; set; }
    public uint ItemId { get; set; }
    public uint ItemarrayId { get; set; }
}
