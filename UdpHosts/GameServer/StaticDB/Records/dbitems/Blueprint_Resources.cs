namespace GameServer.Data.SDB.Records.dbitems;
public record class Blueprint_Resources
{
    public uint BlueprintId { get; set; }
    public uint ItemType { get; set; }
    public uint ItemAttribute { get; set; }
    public uint RsrcQuantity { get; set; }
    public byte IsUnlimited { get; set; }
    public byte IsOutput { get; set; }
    public byte IsRequired { get; set; }
    public byte ResourceStat { get; set; }
}