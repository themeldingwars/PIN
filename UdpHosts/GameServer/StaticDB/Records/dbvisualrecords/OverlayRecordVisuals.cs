namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class OverlayRecordVisuals
{
    public uint VisualrecordId { get; set; }
    public uint RaceFlags { get; set; }
    public uint SexFlags { get; set; }
    public uint OverlayRecordId { get; set; }
    public uint OverlayMaskId { get; set; }
    public byte Slot { get; set; }
}
