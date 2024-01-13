namespace GameServer.Data.SDB.Records.apt;
public record class TargetDifferenceCommandDef
{
    public uint Id { get; set; }
    public byte ReplaceFormer { get; set; }
    public byte SwapCurrentFormer { get; set; }
}