namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class ResourceNodeTypeResource
{
    public uint CenterLow { get; set; }
    public uint CenterHigh { get; set; }
    public uint ItemQualityLow { get; set; }
    public uint EdgeHigh { get; set; }
    public uint ItemId { get; set; }
    public uint ItemQualityHigh { get; set; }
    public uint EdgeLow { get; set; }
    public uint NodeTypeId { get; set; }
}
