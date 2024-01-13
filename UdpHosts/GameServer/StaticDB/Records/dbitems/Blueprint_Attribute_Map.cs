namespace GameServer.Data.SDB.Records.dbitems;
public record class Blueprint_Attribute_Map
{
    public uint ItemAttribute { get; set; }
    public uint BlueprintType { get; set; }
    public uint ComponentType { get; set; }
}
