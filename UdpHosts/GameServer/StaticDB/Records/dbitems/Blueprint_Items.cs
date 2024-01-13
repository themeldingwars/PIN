namespace GameServer.Data.SDB.Records.dbitems;
public record class Blueprint_Items
{
    public uint BlueprintId { get; set; }
    public uint ItemType { get; set; }
    public uint RsrcQuantity { get; set; }
    public byte IsOutput { get; set; }
}
