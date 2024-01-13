namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromBonusCommandDef
{
    public float RegisterVal4 { get; set; }
    public float RegisterVal6 { get; set; }
    public float RegisterVal8 { get; set; }
    public float RegisterVal5 { get; set; }
    public float RegisterVal10 { get; set; }
    public float RegisterVal0 { get; set; }
    public float RegisterVal7 { get; set; }
    public float RegisterVal9 { get; set; }
    public float RegisterVal1 { get; set; }
    public float RegisterVal2 { get; set; }
    public float RegisterVal3 { get; set; }
    public uint Id { get; set; }
    public byte BonusTrackCount { get; set; }
    public byte BonusTrack { get; set; }
}