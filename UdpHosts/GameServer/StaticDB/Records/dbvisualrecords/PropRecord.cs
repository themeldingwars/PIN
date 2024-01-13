namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class PropRecord
{
    public uint AnimationNetworkId { get; set; }
    public uint VisualRecordId { get; set; }
    public uint SinOverlayId { get; set; }
    public uint Id { get; set; }
    public byte IsPlaceholder { get; set; }
}
