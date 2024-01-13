namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class ZoneChunkLinker
{
    public uint Zoneid { get; set; }
    public uint Chunkid { get; set; }
    public byte Clientonly { get; set; }
}
