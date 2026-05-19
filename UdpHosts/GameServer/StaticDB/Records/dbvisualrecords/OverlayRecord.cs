namespace GameServer.StaticDB.Records.dbvisualrecords;
public record class OverlayRecord
{
    public uint DisplayFlags { get; set; }
    public uint Id { get; set; }
    public uint LocalizedNameId { get; set; }
    public byte OnTopOfWarpaint { get; set; }
}