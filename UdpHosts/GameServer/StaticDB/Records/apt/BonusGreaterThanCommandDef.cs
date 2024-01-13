namespace GameServer.Data.SDB.Records.apt;
public record class BonusGreaterThanCommandDef
{
    public uint Id { get; set; }
    public uint GreaterThan { get; set; }
    public byte BonusTrackCount { get; set; }
    public byte BonusTrack { get; set; }
}