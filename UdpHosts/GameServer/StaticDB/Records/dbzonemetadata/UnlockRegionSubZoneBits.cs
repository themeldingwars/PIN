namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class UnlockRegionSubZoneBits
{
    public uint SubzonerecordId { get; set; }
    public uint UnlockregionId { get; set; }
    public byte BitIndex { get; set; }
}
