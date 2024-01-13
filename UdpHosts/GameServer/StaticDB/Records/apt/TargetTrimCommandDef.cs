namespace GameServer.Data.SDB.Records.apt;
public record class TargetTrimCommandDef
{
    public uint Trimsize { get; set; }
    public uint Id { get; set; }
    public byte TrimsizeRegop { get; set; }
    public byte Chomp { get; set; }
    public byte Former { get; set; }
    public byte Current { get; set; }
    public byte FromFront { get; set; }
}