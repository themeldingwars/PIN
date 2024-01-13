namespace GameServer.Data.SDB.Records.dbitems;
public record class LevelBand
{
    public uint Id { get; set; }
    public byte Max { get; set; }
    public byte Min { get; set; }
}
